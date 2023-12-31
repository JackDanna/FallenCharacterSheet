module ContainerList

open FallenLib.Container
open FallenLib.ItemStack
open FallenLib.Item

type Msg =
    | ContainerMsg of int * Container.Msg
    | Insert of string
    | Remove of int

let init () : Container list = []

let update (allItemStackList: ItemStack list) (msg: Msg) (model: Container list) : Container List =
    match msg with
    | ContainerMsg (position, containerMsg) ->
        model
        |> List.mapi (fun index container ->
            if position = index then
                Container.update allItemStackList containerMsg container
            else
                container)
    | Insert itemContainerName ->
        allItemStackList
        |> List.find (fun itemStack -> itemStack.item.name = itemContainerName)
        |> (fun itemStack -> itemStack.item)
        |> itemToItemNameAndContainerClasses
        |> List.map itemNameAndContainerClassToContainer
        |> List.append model
    | Remove position -> List.removeAt position model

open Feliz
open Feliz.Bulma

let view
    (containerItemNameList: string list)
    (itemNameList: string list)
    (model: Container list)
    (dispatch: Msg -> unit)
    =
    [ Bulma.label "Container List:" ]
    @ (List.mapi
        (fun position container ->
            Html.div [
                Container.view itemNameList container (fun msg -> dispatch (ContainerMsg(position, msg)))
                Html.button [
                    prop.onClick (fun _ -> dispatch (Remove position))
                    prop.text "-"
                ]
            ])
        model)
      @ [ Html.div [
              Bulma.input.text [
                  prop.list "listId"
                  prop.onTextChange (fun input -> dispatch (Insert input))
              ]
              Html.datalist [
                  prop.id "listId"
                  prop.children (
                      List.map
                          (fun (containerItemName: string) ->
                              Html.option [
                                  prop.value containerItemName
                              ])
                          containerItemNameList
                  )
              ]
          ] ]
    |> Bulma.container