using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EzBzDownloader.Client.Logic;
using EzBzDownloader.Client.Model;
using EzBzDownloader.Lib;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace EzBzDownloader
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var cli = new CommandLineApplication
            {
                Name = "EzBzDownloader",
                FullName = "Easy Backblaze Downloader",
                Description = "Downloads all pending restores without logging every time"
            };
            cli.HelpOption("-h|--help");

            var config = File.Exists("config.json")
                ? JsonConvert.DeserializeObject<Config>(await File.ReadAllTextAsync("config.json", Encoding.UTF8, cancellationTokenSource.Token))
                    ?? throw new Exception("Failed to parse config.json")
                : new Config();

            var username = cli.Option("-u", "Username", CommandOptionType.SingleValue);
            var password = cli.Option("-p", "Password", CommandOptionType.SingleValue);
            var totpKey = cli.Option("-k", "TOTP Key", CommandOptionType.SingleValue);
            var save = cli.Option("--save", "Saves the given username/password/totp key for use on next launch", CommandOptionType.NoValue);

            config.Username = username.HasValue()
                ? username.Value()
                : Prompt.GetString("Enter your username:");
            config.Password = password.HasValue()
                ? password.Value()
                : Prompt.GetPassword("Enter your password:");

            config.SecretKey = totpKey.HasValue()
                ? totpKey.Value()
                : Prompt.GetPassword("Enter your TOTP key (optional - if not specified but 2FA is required, will prompt for TOTP code):");

            if (username.HasValue())
                config.Username = username.Value();
            if (password.HasValue())
                config.Password = password.Value();
            if (totpKey.HasValue())
                config.SecretKey = totpKey.Value();

            if (string.IsNullOrWhiteSpace(config.Username))
                throw new Exception("Username is required");
            if (string.IsNullOrWhiteSpace(config.Password))
                throw new Exception("Password is required");

            if (save.HasValue())
                await File.WriteAllTextAsync("config.json", JsonConvert.SerializeObject(config), Encoding.UTF8, cancellationTokenSource.Token);

            cli.OnExecuteAsync(_ => ConsoleHost.WaitForShutdownAsync(ct => OnExecute(config, ct)));

            return await cli.ExecuteAsync(args, cancellationTokenSource.Token);
        }

        private static async Task<int> OnExecute(Config config, CancellationToken cancellationToken)
        {
            var client = new BzDownloadClient(config.Username, config.Password, config.SecretKey, message => Prompt.GetString(message));
            var restores = await client.ListRestoresAsync(config.Username, cancellationToken);
            if (restores == null)
            {
                Console.WriteLine("No restores available.");
                return 0;
            }

            foreach (var restore in restores)
            {
                Console.WriteLine($"Downloading {restore.DisplayFilename} ({restore.Zipsize / 1024 / 1024:N}MB)");
                await client.DownloadRestoreAsync(restore, "", cancellationToken);
            }

            return 0;
        }
    }
}