# DataStore

A Document-Centric Data Access Framework for Azure DocumentDB

##Overview

DataStore is an easy-to-use, data-access framework, which maps POCO C# classes to documents.

It supports basic CRUD operations on any C# object, with some additional features such as:

* Strongly typed mapping between documents and C# class types with generics
* Support for LINQ queries against objects and their children (where the DocumentDB client supports it)
* In-memory database, and event history for testing
* Managing of Ids and timestamps of an object hierarchy for you
* Automatic retries of queries when limits are exceeded

			
DataStore is built with .NET Core SDK v.1.0.0-preview2-003131 tools but requires TFM net451. 

This is mainly because the DocumentDB Client Library does not support .NET Core yet.

##Coming Soon

* Limited cross-document transactional support
* Partition support 

##Usage

Import the Nuget Package "DataStore".

Create a C# class which inherits DataStore.DataAccess.Models.Aggregate.
```
class Car : Aggregate {
	public string Make { get; set; }
	public string Model { get; set; }
}
```
Create a new DataStore object.
```
var d = new DataStore(new DocumentRepository(new DocumentDbSettings(
            string authorizationKey, 
            string databaseName, 
            string defaultCollectionName, 
            string endpointUrl)
			));
```
Save it to the database.

`var car = d.Create(new Car() { Make = "Toyota", Model = "Corolla"});`

Update it 

`d.UpdateById<Car>(car.id, (car) => car.Model = "Celica");`

or
```
car.Model = "Celica";
d.UpdateUsingValuesFromAnotherInstanceWithTheSameId(car);
```

Delete It

`d.SoftDeleteById<Car>(car.Id);`

Find It

`var toyotaCars = d.Read<Car>(query => query.Where(c => c.Model = "Toyota"));`

or

`var myToyota = d.ReadActiveById<Car>(car.Id);`

See IDataStore.cs for the full list of supported methods.