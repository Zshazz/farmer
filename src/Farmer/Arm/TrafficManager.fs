[<AutoOpen>]
module Farmer.Arm.TrafficManager

open Farmer

let profiles = ResourceType ("Microsoft.Network/trafficManagerProfiles", "2018-04-01")
let endpoints = ResourceType ("Microsoft.Network/trafficManagerProfiles/azureEndpoints", "2018-04-01")
let externalEndpoints = ResourceType ("Microsoft.Network/trafficManagerProfiles/externalEndpoints", "2018-04-01")

type RoutingMethod =
    | Performance
    | Weighted
    | Priority
    | Geographic
    | Subnet

type MonitorProtocol =
    | Http
    | Https

type MonitorConfig =
    { Protocol : MonitorProtocol
      Port: int
      Path: string
      IntervalInSeconds: int
      ToleratedNumberOfFailures: int
      TimeoutInSeconds: int }

type Endpoint =
    { Name : ResourceName
      Status: FeatureFlag
      Target : string
      Weight: int
      Priority: int
      Location: Location }
    interface IArmResource with
        member this.ResourceId = endpoints.resourceId (this.Name)
        member this.JsonModel =
            {| location = this.Location
               properties =
                {| endpointStatus = this.Status.ToString()
                   weight = this.Weight
                   priority = this.Priority
                   endpointLocation = this.Location.ArmValue
                   targetResourceId = match this.Target with
                                      | ExternalDomain _ -> null
                                      | WebSite resource -> ArmExpression.resourceId(sites, resource).Eval()
                   target = match this.Target with
                            | ExternalDomain domain -> domain
                            | WebSite resource -> resource.Value |} |} :> _

type Profile =
    { Name : ResourceName
      DnsTtl: int
      Status : FeatureFlag
      RoutingMethod : RoutingMethod
      MonitorConfig : MonitorConfig
      TrafficViewEnrollmentStatus : FeatureFlag
      Endpoints : Endpoint list
      DependsOn : ResourceName list
      Tags: Map<string,string> }
    interface IArmResource with
        member this.ResourceId = profiles.resourceId (this.Name)
        member this.JsonModel =
            {| name = this.Name.Value
               location = "global"
               dependsOn = this.DependsOn |> List.map (fun r -> r.Value)
               tags = this.Tags
               properties =
                   {| profileStatus = this.Status.ToString()
                      trafficRoutingMethod = this.RoutingMethod.ToString()
                      trafficViewEnrollmentStatus = this.TrafficViewEnrollmentStatus.ToString()
                      dnsConfig = {| relativeName = this.Name.Value.ToString()
                                     ttl = this.DnsTtl |}
                      monitorConfig = {| protocol = this.MonitorConfig.Protocol.ToString().ToUpperInvariant()
                                         port = this.MonitorConfig.Port
                                         path = this.MonitorConfig.Path
                                         intervalInSeconds = this.MonitorConfig.IntervalInSeconds
                                         toleratedNumberOfFailures = this.MonitorConfig.ToleratedNumberOfFailures
                                         timeoutInSeconds = this.MonitorConfig.TimeoutInSeconds |}

                      endpoints = this.Endpoints |> List.map (fun e -> (e:>IArmResource).JsonModel) |}
            |} :> _