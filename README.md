# CheckerApi

## Purpose
The Bot is designed to monitor NiceHash API for bids and send IFTTT notifications if the predefined conditions are met.

The Bot is .Net Core 2.2 API using Entity Framework(EF) to connect to MySql. The EF creates the Database (DB) with the needed tables, columns, indexes and so on.

## Repo 
https://github.com/Vutov/CheckerApi

## Requirements
At least 1 shared CPU
At least 250mb ram
At least 20mb for the App and 100mb for the DB + 250mb per day for audit data (if we want to store 7 days, 250*7mb)
IFTTT webhook

## Initial Setup 
### Framework-dependent Deployment
(https://docs.microsoft.com/en-us/dotnet/core/deploying/)

```
Download latest SDK and Runtime for the desired OS - https://www.microsoft.com/net/download
Download MySQL - https://www.mysql.com/downloads/
Clone the Repo
Create appsettings.Production.json with the settings you desire
Setup environment variables
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://*:63139
Restore packages - ‘dotnet restore’
Update the DB - ‘dotnet ef database update’
Start the bot - ‘dotnet run’
```

### Self-contained Deployment
.Net Core 2 offers self-contained deployment. For that you will need to compile the code from this repo with Visual Studio 2017 (latest). You will also need latest dotnet SDK and Runtime.

## appsettings.Production.json example

```json
{  
   "ConnectionStrings":{  
      "Connection":"server=localhost;uid=root;pwd=123456;database=CheckerAPI;SslMode=none"
   },
   "Trigger":{  
      "Domain":"http://maker.ifttt.com/trigger/",
      "Uri":"test-alert/with/key/<>"
   },
   "Api":{  
      "Password":null,
      "Alert":{  
         "Message":"Notify immediately!!! @channel",
         "IntervalMin":10
      },
      "ClearAuditMinutes":1000000
   },
   "NiceHash": {
    "Url": "https://api.nicehash.com/",
    "Request": "api?method=orders.get&location={location}&algo=36",
    "Locations": [ 0, 1 ]
   },
   "Pool" : {
    "Url": null,
    "Request":  null 
   },
   "Kestrel":{  
      "Certificates":{  
         "Default":{  
            "Subject":"localhost",
            "Store":"My",
            "Location":"CurrentUser",
            "AllowInvalid":"false"
         }
      }
   }
}
```
*Set AllowInvalid to “true” to allow invalid certificates (e.g. self-signed)*

Under Trigger is the IFTTT trigger of the webhook in IFTTT.
Under Api are the settings of the bot. The message is the 3rd part of the message, it will be sent only once every IntervalMin minutes.
Under Kestrel are the setting for the build in dotnet web server.

### Kestrel configuration example for Linux

```json
{  
   "Kestrel":{  
      "Certificates":{  
         "Default":{  
            "Path":"/home/kamigawa/Desktop/https.pfx",
            "Password":"123"
         }
      }
   }
}
```

Note: Kestrel is not made for Production use, it does support https, but its good practice to have full blown server like IIS or ngnix and do reverse proxy to Kestrel

## Bot functionality
*{url}/* - shows metadata for the bot -

>{“status”:“Running”,“foundOrders”:3162,“auditCount”:1683106,“config”:[“AcceptedSpeed (0.02)”,“LimitSpeed (11)”,“PriceThreshold (0.03)”,“LastNotification (6/12/18 8:49:39 AM)”,“MinimalAcceptedSpeed (0.003)”,“AcceptedPercentThreshold (0.1)”,“EnableAudit (True)”],“conditions”:[“AcceptedSpeedCondition (True)”,“SignOfAttackCondition (True)”,“PercentThresholdCondition (True)”]}

>*Config* - shows the current values of the configuration settings

>*Conditions* - show current conditions and are they enabled

*{url}/swagger* - shows Swagger UI

*{url}/version* - shows the last boot, environment and name

*{url}/testnotifications/{password}* - send test notification to verify the flow works

*{url}/data* - shows last 10 recorded orders matching the criteria

*{url}/data/{any}* - shows last {any} recorded orders matching the criteria

*{url}/data/audit* - shows last 1000 recorded audit orders

*{url}/data/audit.csv* - returns last 1000 recorded audit orders as csv file

*{url}/data/audit.zip* - returns last 1000 recorded audit orders as zip of csv file

>For audit and audit.csv to add filtering {url}/data/audit?top=10&from=2018-06-07T17:00:00&id=123&to=2018-06-07T18:00:00 . All of the 4 are optional. For example {url}/data/audit?id=10
‘from’ and ‘to’ are ‘yyyy-mm-ddThh:mm:ss’ in UTC

*{url}/acceptedspeed/{value}/{password}* - sets {value} for AcceptedSpeed

*{url}/limitspeed/{value}/{password}* - sets {value} for LimitSpeed

*{url}/pricethreshold/{value}/{password}* - sets {value} for PriceThreshold

*{url}/minimalacceptedspeed/{value}/{password}* - sets {value} for MinimalAcceptedSpeed

*{url}/acceptedpercentthreshold/{value}/{password}* - sets {value} for AcceptedPercentThreshold

*{url}/totalhashthreshold/{value}/{password}* - sets {value} for TotalHashThreshold

*{url}/enableaudit/{value}/{password}* - sets {value} for EnableAudit / ‘true’ or ‘false’ with words

*{url}/condition/AcceptedSpeedCondition/{value}/{password}* - sets {value} for AcceptedSpeedCondition

*{url}/condition/SignOfAttackCondition/{value}/{password}* - sets {value} for SignOfAttackCondition

*{url}/condition/PercentThresholdCondition/{value}/{password}* - sets {value} for PercentThresholdCondition

*{url}/condition/TotalMarketCondition/{value}/{password}* - sets {value} for TotalMarketCondition

*{url}/condition/CriticalTotalMarketCondition/{value}/{password}* - sets {value} for CriticalTotalMarketCondition

>urls are not case sensitive, value is ‘true’ or ‘false’
{password} - its set in the config under password field.

## Bot Conditions

### For Active orders:

**Large order** - alert if any order’s accepted speed is above {acceptedspeed} configuration. (Reason: single orders of large size can indicate an attacker’s bid has grown to threatening size.)

**Suspicious order** - alert if any order has no limit or a limit greater than {limitspeed} AND its price is within {pricethreshold} the top order (sorted by price) AND accepted speed is above {minimalAcceptedSpeed}. (Reason: normal bidders rarely price their orders near the top of the order book, and if they do so, they generally include small power limits; malicious attackers need to rapidly accumulate enough hashpower to reach attack scale and must place their bids high. New orders priced high with no power limits are suspicious and should be watched.)

**Suspicious percentage order** - alert if any order has no limit or a limit greater than {limitspeed} AND its price is above the price of the Benchmark order* sorted by price AND accepted speed is above {minimalAcceptedSpeed}. (Reason: this provides an improved rubric for what defines a “high” price to avoid false alerts during times when the active order book spread is naturally tight or the order book has an unusual distribution.)

**Benchmark order** is the order after which the sum of accepted speed for sorted-by-price orders is more than the {percent} of total accepted speed for the batch of orders. For example, if the threshold percentage is 90%, and the total Accepted Speed (hashpower delivered) of all orders is 1000 (i.e., 1000 units of power are being delivered across all actives order), and we count up from the lowest-priced order until we account for a total 900 Accepted Speed, the price of the next unit of power delivered is the Benchmark order price because 90% of the power being delivered is purchased at a lower price, and prices over this Benchmark are considered “high.”

**Total Market** - alert if the total market orders (all markets combined) accepted speed is above {totalhashthreshold} percentage of current network rate (taken from pool). Calculation formula *Sum(all active orders' accepted speed) * 1 000 000 * {totalhashthreshold} >= {networkrate}*

**Critical Total Market** - alert if the total market orders (all markets combined) accepted speed is above current network rate (taken from pool). Calculation formula *Sum(all active orders' accepted speed) * 1 000 000 >= {networkrate}*

Note: If Critical Total Market Condition alert is sent Total Market Condition will not be send

## Notifications

**Large order notification** - 51% attack may be underway

**Suspicious order alert** - an attack might develop soon

**Suspicious order progress** - less information for suspicious order that is still active

**Total Market alert** - the network can be attacked, offered hash rate is close to the network's hash rate

**Critical Total Market alert** - offered hash rate is over the network's hash rate, it is quite possible to buy an attack

**Test notification** - ignore

>Note: Notifications are made of 3 messages - 1st message is bid alert, 2nd is bid conditions and 3rd message is for warning. For example @channel and the 3rd message will be included only once every 10 minutes to avoid the spam.

## Architecture of the bot
Api with background thread checking the NiceHash API every 30 seconds. NiceHash Api updates every 30 seconds. If order is flagged by any of the conditions it is saved to MySql DB for further analysis. The API triggers IFTTT webhook, that can trigger other hooks for notifications via email, slack and others.

## Details
The data is gettered from https://api.nicehash.com/api?method=orders.get&location={location}&algo=24 , where location is 0 or 1 (EU and US). Once the data is available it is run against the predefined conditions using set of properties of the bid. The bid is checked if is Active, for Large Order is used accepted_speed, for suspicious bid is used limit_speed and price (more details about the condition logic in Condition section). Found orders are stored in MySql for future analisys. Trigger is sent to http://maker.ifttt.com/trigger/ for each order. IFTTT aplet then handles the webhook.

Auto-registry of Conditions. Currently there are 2 types of conditions - for market or for all markets - Using the attribute - [Condition(10)] or [GlobalCondition(4)]with giving priority value and inheriting Condition abstract class will automaticly register the condition in the DB on startup, enable it and start checking against it.

Priority of the condition - one order can match multiple conditions, to avoid multiple entries in the db and multiple alerts for the same order we use priority. It means that the alert message with be for the condition with bigger (smaller number) priority. Example - If Large order has bigger priority then bid percentage - the message sent in the alert will be for Large Order, not percentage bid.

Entity Framework Entities - Currently there is no Reposity pattern, DBContext is used with the DBSets. There are 2 types of DBSets - ‘normal’ ones and with the sufix ReadOnly. Second have detached entities and any change to them will not be affected in the DB, unless attached back before the save.

.NET Quartz - Job Scheduler, Jobs:

-Pull job - is used to schedule every 30 seconds pull of data from NiceHash API and an other job again every 30 seconds to clean the DB of old audits (older then 30 hours).

-Cleaner job - Currently using Inline SQL to do the Delete of old audit rows (older then 25 hours), it handles 1mil rows in 10secs or so. Job is run every 30 secs to keep the data low. 

-Zip job - Run once a day at 24:00 to zip all audits of the previous day (00:00:00 - 23:59:59) in /AuditZips folder.

-NetworkHashrateJob - Run once every 5 minutes and gathers network rate from a pool. If no pool is nofigured in appsettings the job will not start.

## Releases:
1.2.0.0 - 25.01.2019
```
Added Total Market Condition and Critical Total Market Condition

Introduced Global Condition - combining all markets and doing the condition on top

Extracted URLs to appsettings

Upgraded to Core 2.2

Added Swagger

Minor bug fixes

Minor code refactoring
```
1.1.7.0 - 18.06.2018
```
Added HSTS and upgrade on the way to use SSL with Cert.

Added Automatic zip of audit data for last day

Added Endpont to serv zip csv data

Added StyleCop

Reducing the audit orders to only Active with AcceptedSpeed > 0, decreased their number by 65%

Improved logs
```
1.1.6.0 Conditions changes - 11.06.2018
```
Added SSL for the Bot

New settings for disabling conditions

New setting and check for {minimal accepted speed} added to Suspicious condition to reduce the white noice in slack about suspicios bids bellow {minimal accepted speed} msol (most likely 1 to 3)

New condition PercenteageThreshold added:

Sum all the delivered order powers in the server (aka acceptedSpeed).
Multiply by .1 to find the “10 percent” threshold, “TPT.”
Sort all orders by price.
Count from top price order downwards, summing up the acceptedSpeed as a RunningTotal.
When you reach the point where RunningToal > TPT, stop.
The price of that order is the new ReportingThreshold.

Added full audit on all orders placed (last 7 days are kept)

Added CSV export for audit data
```
1.1.5.0 Suspicious condition change - 04.06.2018
```
Suspicious order condition changed to - any active order with no limit or above {speedLimit} witthin {priceTreshold} of top order (the order with biggest price)
```
