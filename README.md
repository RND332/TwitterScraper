# TwitterScraper
Tool for working with Twitter Users/Tweets via Nitter Container, and put them in Google Sheets, with Telegram Bot
---------------
### Dependencies
#### 1) Nitter node on localhost:8080
---------------
#### Extensions Folder
##### Extensions.cs 
##### Add ```WithoutFirstItem(this List<string>)``` and ```WithoutFirstItem(this IList<string>)``` extension for simple scraping google spreadsheet
##### Add ```GetLinks(this List<Tweet> list)``` for simple get all tweets links from List<Tweet> 
---------------
#### Google Folder
##### GoogleWorker.cs
##### ```GoogleClient``` Create new object for read/write/clean/newtable functions
##### ```ReadSheet``` Read Sheet data with specifited range
##### ```NewTable``` Create New Table table with specifited name
##### ```ClearTable``` Clear table with specifited name
##### ```WriteSheet``` Write data in specifited sheet
---------------
