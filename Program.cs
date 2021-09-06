using AngleSharp;

var client = new HttpClient();
client.BaseAddress = new Uri("https://prnt.sc/");
client.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.28.2");
client.DefaultRequestHeaders.Add("Accept", "*/*");
client.DefaultRequestHeaders.Add("Connection", "keep-alive");

var seq =
    Enumerable
        .Range(97, 26)
        .SelectMany(f =>
            Enumerable
                .Range(97, 26)
                .SelectMany(s =>
                    Enumerable
                        .Range(0, 9999)
                        .Select(t => $"{(char) f}{(char) s}{t:0000}")
                )
        );

var dataPath = "Data";
if (!Directory.Exists(dataPath)) {
    Directory.CreateDirectory(dataPath);
}

bool FileExists(string itemName) =>
    Directory.EnumerateFiles(dataPath).Any(f => f.EndsWith($"*{itemName}.png"));

foreach (var item in seq) {
    var filePath = Path.Combine(dataPath, $"{item}.png");

    if (!FileExists(filePath)) {
        try {
            var response =
                await client.GetStringAsync(item);

            var context =
                BrowsingContext.New(Configuration.Default);

            var document =
                await context.OpenAsync(
                    req => req.Content(response));

            var imageLink =
                document.QuerySelector("#screenshot-image")
                    .GetAttribute("src");

            if (imageLink.Contains("imgur")) {
                Console.WriteLine($"{item} Missing Screenshot\n\n");
                continue;
            }

            var imageResponse =
                await client.GetAsync(imageLink);

            using var fs = new FileStream(filePath, FileMode.CreateNew);
            await imageResponse.Content.CopyToAsync(fs);
        }
        catch (Exception ex) {
            Console.WriteLine($"Error downloading {item}");
            Console.WriteLine(ex.Message);
        }
    }
}
