using HtmlAgilityPack;
using System.Net;

string hrefVal = string.Empty;
int num = 0;

Console.WriteLine("paste site you want to crawl on: ");
string http = Console.ReadLine();

if (string.IsNullOrEmpty(http)) {
    Console.WriteLine("Cannot be empty!");
    return;
}

Uri uriResult;

bool result = Uri.TryCreate(http, UriKind.Absolute, out uriResult) 
    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

if (!result) {
    Console.WriteLine("Not valid uri!");
    return;
}

List<string> disallowedPaths = new List<string>();
try {
    string robotsUrl = $"{uriResult.Scheme}://{uriResult.Host}/robots.txt";

    using (var client = new WebClient()) {
        string robotsContent = client.DownloadString(robotsUrl);
        var lines = robotsContent.Split('\n');

        bool appliesToAll = false;
        
        foreach(var line in lines) {
            var trimmed = line.Trim();

            if(trimmed.StartsWith("User-agent:")) {
                appliesToAll = trimmed.Contains("*");            
            }

            if (appliesToAll && trimmed.StartsWith("Disallow:")) {
                var disallowedPath = trimmed.Substring("Disallow:".Length).Trim();
                if (!string.IsNullOrEmpty(disallowedPath)){
                    disallowedPaths.Add(disallowedPath);
                }
            }
        }
    }
} catch (Exception e) {
    Console.WriteLine(e);
}

HtmlWeb web = new HtmlWeb();

var htmlDoc = web.Load(http);

var htmlNodes = htmlDoc.DocumentNode.SelectNodes("//a[@href]");

bool IsDisallowed(string url) {
    try {
        Uri hrefUri = new Uri(uriResult, url);
        string path = hrefUri.AbsolutePath;

        foreach (var rule in disallowedPaths) {
            if (path.StartsWith(rule.TrimEnd('*'))) {
                return true;
            }
        }
    } catch {
        return true;
    }
    return false;
}

if (htmlNodes != null) {
    foreach (var node in htmlNodes) {

        hrefVal = node.GetAttributeValue("href", string.Empty);

        if (string.IsNullOrEmpty(hrefVal)) continue;

        if (!IsDisallowed(hrefVal)) {
            num++;
            Console.WriteLine(num + ": " + hrefVal + "\n");
        }

    }
} else {
    Console.WriteLine("No tags found!");
}
