#r @"../libs/Newtonsoft.Json.dll"
#r @"../../../src/Farmer/bin/Debug/netstandard2.0/Farmer.dll"

open Farmer
open Farmer.Builders

let plan = servicePlan {
    name "theFarm"
    sku WebApp.Sku.F1
}

let ai = appInsights {
    name "insights"
}

let planets = [ "jupiter"; "mars"; "pluto"; "venus" ]

let webApps : IBuilder list = [
    for planet in planets do
        webApp {
            name ("mywebapp-" + planet)
            link_to_service_plan plan
            link_to_app_insights ai
        }
]

let template = arm {
    location Location.NorthEurope
    add_resource plan
    add_resource ai
    add_resources webApps
}

template.ToFile "my-resource-group-name"