# WikiMedia
WikiMedia is a console application built on C# with .NET Core.

This application permits you to download files from Wiki Media Page Views (https://dumps.wikimedia.org/other/pageviews/), then process them selecting `page_views` with `max_views` for each `domain_code` for an hour and print them on the console.

### Considerations
- The `page_views` have some special characters. So, for that reason use WindowsTerminal to see them.
- Wiki Media Page Views doesn't support more than 3 requests at the time. For that reason use Polly to manage the retry request.

### Configurations
- You can configure the hour you need to start to download and the number of last hours to download. 

### Third-Party Dependencies
- FakeItEasy
- FluentAssertions
- Polly
- SharpZipLib

### Docker
- The image on Docker is https://hub.docker.com/r/emilymitacc/wikimedia
