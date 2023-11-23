# tenject
This is a simple implementation of a DI container that uses reflection and attributes to resolve dependencies in fields, properties, methods, and constructors.

## Container
All the data can be stored and retreived from container
```csharp
using Te.DI;

...

var container = new TenjectContainer();
```
Now you can use a container to perform the following set of operations:

1. **Resolve** - Resolve all dependencies marked by the `[Inject]` attribute.
```csharp
// Resolve an existing instance's dependencies
var someInstance = new SomeClass();
container.ResolveInstance<SomeClass>(someInstance);

// Create and resolve new instance
var someInstance = container.ResolveNew<SomeClass>();
```
2. **Bind** - add a new type binding to the container. Binding can be done to an interface or to an actual class. Binding also automatically performs a resolve. Bind calls that create new instances resolve [Inject]-marked constructors, while instance ones don't.
```csharp
// Bind existing instance to its own type
var someInstance = new SomeClass();
container.BindInstance<SomeClass>(someInstance);

// Bind existing instance to interface
var someInstance = new SomeClass();
container.BindInstance<SomeClass, ISomeInterface>(someInstance);

// Create and bind new instance
var someInstance = container.BindNew<SomeClass>();

// Create and bind new instance
var someInstance = container.BindNew<SomeClass, ISomeInterface>();
```
3. **Get binding** - pass the type and get a bound instance if it exists.
```csharp
var someInstance = container.GetBinding<ISomeInterface>();
```
All of the operations can be performed on an existing instance or create a new instance of the requested type.

## Inject attribute
Any field, property, method, and constructor can be marked with `[Inject]`. This means that they will be populated with data from the container during the resolve stage.
```csharp
// Inject field
[Inject] private ISomeInterface resolvedField;

// Inject property
[Inject] private ISomeInterface ResolvedProperty { get; set; }

// Inject constructor
[Inject] private ResolvedClass(ISomeInterface someInstance, IAnotherInterface anotherInstance)
{
  ...
}

// Inject method
[Inject] private void ResolvedMethod(ISomeInterface someInstance, IAnotherInterface anotherInstance)
{
  ...
}
```

## Additional details
* Constructor injection works only when the container creates an instance itself through a `BindNew`/`ResolveNew` call. If the requested injection type is not bound in the container, an exception will be thrown.
* If there is no `[Inject]`-marked constructor, `BindNew`/`ResolveNew` will call the default parameterless constructor instead.
* There are two options to call the container operations: a generic method and a regular method that requires the `Type` as a parameter.

Order of injection:
  1. Constructors
  2. Methods
  3. Fields
  4. Properties

## Limitations and future improvements
* Add support for resolving recursive generic types. At this point, the container will fail with the resolution of the following class:
```csharp
class SomeGenericClass<T>
{
  [Inject] public List<T> SomeList;
}
```
