﻿namespace CollectionSample

open System
open Gjallarhorn
open Gjallarhorn.Bindable

module Requests = 
    type Model = Request list

    // These are the updates that can be performed on our requests
    type Message = 
        | Update of newRequest : Request * originalRequest : Request
        | AddNew of Guid * float
        | Remove of Request list

    // Update the model based on an UpdateRequest
    let rec update msg current =
        let excluded r = current |> List.filter (fun req -> req.Id <> r.Id)
        match msg with
        | Update(r,o)-> r :: excluded o
        | AddNew(guid, hours) -> Request.create guid hours :: current 
        | Remove(toRemove) -> current |> List.except toRemove

    type RequestsViewModel =
        {
            Requests : Model
            Edit : VmCmd<CollectionNav>
        }
    let reqsd = { Requests = [] ; Edit = Vm.cmd <| CollectionNav.DisplayRequest (Signal.constant Request.defRequest) }
    
    // Map our child component to our navigation model (in this case, by just suppressing child navigation requests)
    let requestComponentWithNav = Request.requestComponent |> Component.suppressNavigation

    // Create the component for the Requests as a whole.
    // Note that this uses BindingCollection to map the collection to individual request -> messages,
    // using the component defined previously, then maps this to the model-wide update message.
    let requestsComponent = //source (model : ISignal<Requests>) =
        let sorted (requests : Model) = requests |> List.sortBy (fun r -> r.Created)        
                   
        Component.create<Model,CollectionNav,Message> [
            <@ reqsd.Requests @> |> Bind.collection sorted requestComponentWithNav Update            
            <@ reqsd.Edit @> |> Bind.cmdParam CollectionNav.DisplayRequest |> Bind.toNav
        ]         
