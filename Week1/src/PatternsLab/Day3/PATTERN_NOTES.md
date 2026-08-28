# Task 1.9 — Short Notes: 8 More Patterns

Each note is 3–5 lines plus one concrete use case.

## Builder
Separates the step-by-step construction of a complex object from its final
representation, so the same construction process can produce different
configurations. A fluent `PizzaBuilder.WithCheese().WithTopping("olives").Build()`
avoids telescoping constructors with a dozen optional parameters.
**Use case:** assembling an `HttpRequestMessage` or a complex SQL query object
piece by piece before executing it.

## Prototype
Creates new objects by cloning an existing "prototype" instance rather than
instantiating a class from scratch, useful when construction is expensive or
the exact runtime type isn't known until execution. C# supports this directly
via `ICloneable` or a copy constructor.
**Use case:** duplicating a fully-configured `GameCharacter` (stats, inventory,
buffs) to spawn a new enemy instead of rebuilding it field by field.

## Decorator
Wraps an object in another object implementing the same interface to add
behaviour dynamically, without subclassing every combination of features.
Decorators can be stacked at runtime.
**Use case:** wrapping a `Stream` with `BufferedStream`, then `GZipStream`,
then `CryptoStream` — each adds a capability without changing the others.

## Command
Encapsulates a request (method call + arguments) as an object, so it can be
queued, logged, undone, or passed around like data instead of being invoked
immediately.
**Use case:** a text editor's undo/redo stack, where each edit is an
`ICommand` with `Execute()` and `Undo()`.

## Template Method
Defines the skeleton of an algorithm in a base class method, deferring
specific steps to overridable methods in subclasses — the overall sequence
stays fixed while individual steps vary.
**Use case:** a `DataImporter` base class fixes the flow
`OpenSource -> ParseRecords -> Validate -> Save`, while `CsvImporter` and
`JsonImporter` only override `ParseRecords`.

## Mediator
Centralizes how a set of objects communicate so they don't reference each
other directly, reducing a tangled many-to-many dependency graph to a
one-to-many relationship with the mediator.
**Use case:** a chat room object that routes messages between `User` objects,
so users never hold references to each other.

## Chain of Responsibility
Passes a request along a chain of handler objects until one of them handles
it, decoupling the sender from knowing which handler will actually process
it.
**Use case:** an HTTP middleware pipeline (`AuthMiddleware -> LoggingMiddleware
-> RoutingMiddleware`), where each handler can process the request or pass it
to the next.

## State
Lets an object change its behaviour when its internal state changes, by
delegating state-specific behaviour to separate state objects instead of
branching on a status field everywhere.
**Use case:** an `Order` whose `Cancel()`/`Ship()` behaviour differs between
`PendingState`, `ShippedState`, and `DeliveredState` objects.
