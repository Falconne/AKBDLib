﻿using AKBDLib.Logging;
using AKBDLib.Util;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace AKBDLib.ResourceResolver
{
    public sealed class Resolver
    {
        private static readonly Lazy<Resolver> Instance =
            new Lazy<Resolver>(() => new Resolver());

        public static Values Values => Instance.Value._values;

        private readonly Values _values;

        public static Credential GetCredential(string name)
        {
            var result = Values.Credentials.FirstOrDefault(c => c.Name == name);
            if (result == null)
            {
                throw new Exception(
                    $"Resource Resolver: Credential set {name} not found!");
            }

            Wrap.Info($"Returning credentials for user {result.Username}");

            return result;
        }

        public static string GetShare(string name)
        {
            Wrap.Info($"Resolving share path for '{name}'");
            var result = Values.Shares.FirstOrDefault(s => s.Name == name);
            if (result == null)
            {
                throw new Exception(
                    $"Resource Resolver: Share {name} not found!");
            }

            if (result.NeedsCredentials())
            {
                void Login()
                {
                    if (result.ForceLogin)
                    {
                        Shell.RunAndGetExitCode(
                            "net",
                            $"use /d {result.Path}");
                    }
                    else if (Directory.Exists(result.Path))
                    {
                        return;
                    }

                    var creds = GetCredential(result.CredentialsName);
                    Shell.RunAndFailIfNotExitZero(
                        "net",
                        $"use {result.Path} /user:{creds.Username} {creds.Password}");
                }
                Login();
            }

            if (!Directory.Exists(result.Path))
            {
                throw new Exception($"Unable to access {result.Path}.");
            }

            Wrap.Info($"Returning share path {result.Path}");
            return result.Path;
        }

        private Resolver()
        {
            var resourceLocatorFile = GetResourceLocatorFile();
            _values = JsonConvert.DeserializeObject<Values>(File.ReadAllText(resourceLocatorFile));
        }

        private static string GetResourceLocatorFile()
        {
            var directoryToCheck = AppDomain.CurrentDomain.BaseDirectory;
            if (directoryToCheck == null)
            {
                throw new Exception("Unable to work out location of running executable");
            }

            while (true)
            {
                var possibleJsonLocation = Path.Combine(directoryToCheck, "SharedTools", "resources.json");
                if (File.Exists(possibleJsonLocation))
                    return possibleJsonLocation;

                directoryToCheck = Directory.GetParent(directoryToCheck)?.FullName;
                if (directoryToCheck == null)
                {
                    throw new Exception("Unable to find SharedTools/resources.json");
                }
            }
        }
    }
}