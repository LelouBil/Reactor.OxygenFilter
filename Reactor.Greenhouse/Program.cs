﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reactor.Greenhouse.Setup;
using Reactor.Greenhouse.Setup.Provider;
using Reactor.OxygenFilter;

namespace Reactor.Greenhouse
{
    internal static class Program
    {
        private static Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Option<bool>("steam"),
                new Option<bool>("itch"),
            };

            rootCommand.Handler = CommandHandler.Create<bool, bool>(GenerateAsync);

            return rootCommand.InvokeAsync(args);
        }

        public static async Task<int> GenerateAsync(bool steam, bool itch)
        {
            var gameManager = new GameManager();

            try
            {
                await gameManager.SetupAsync(steam, itch);
            }
            catch (ProviderConnectionException e)
            {
                Console.WriteLine($"Error downloading version : {e.Message}");
                return 1;
            }


            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = ShouldSerializeContractResolver.Instance,
            };

            Console.WriteLine($"Generating mappings from {gameManager.PreObfuscation.Name} ({gameManager.PreObfuscation.Version})");
            using var old = ModuleDefinition.ReadModule(File.OpenRead(gameManager.PreObfuscation.Dll));

            if (steam)
            {
                await GenerateAsync(gameManager.Steam, old);
            }

            if (itch)
            {
                await GenerateAsync(gameManager.Itch, old);
            }

            return 0;
        }

        private static async Task GenerateAsync(Game game, ModuleDefinition old)
        {
            Console.WriteLine($"Compiling mappings for {game.Name} ({game.Version})");

            using var moduleDef = ModuleDefinition.ReadModule(File.OpenRead(game.Dll));
            var version = game.Version;
            var postfix = game.Postfix;

            var generated = Generator.Generate(old, moduleDef);

            await File.WriteAllTextAsync(Path.Combine("work", version + postfix + ".generated.json"), JsonConvert.SerializeObject(generated, Formatting.Indented));

            Apply(generated, Path.Combine("Mappings", "universal.json"));
            Apply(generated, Path.Combine("Mappings", version + postfix + ".json"));

            generated.Compile(moduleDef);

            Directory.CreateDirectory(Path.Combine("Mappings", "bin"));
            await File.WriteAllTextAsync(Path.Combine("Mappings", "bin", game.Name.ToLower() + ".json"), JsonConvert.SerializeObject(generated));
        }

        private static void Apply(Mappings generated, string file)
        {
            if (File.Exists(file))
            {
                var mappings = JsonConvert.DeserializeObject<Mappings>(File.ReadAllText(file));
                generated.Apply(mappings);
            }
        }

        public class ShouldSerializeContractResolver : CamelCasePropertyNamesContractResolver
        {
            public static ShouldSerializeContractResolver Instance { get; } = new ShouldSerializeContractResolver();

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                if (property.PropertyType != null && property.PropertyType != typeof(string))
                {
                    if (property.PropertyType.GetInterface(nameof(IEnumerable)) != null)
                    {
                        property.ShouldSerialize = instance => (instance?.GetType().GetProperty(property.UnderlyingName!)!.GetValue(instance) as IEnumerable<object>)?.Count() > 0;
                    }
                }

                return property;
            }
        }
    }
}
