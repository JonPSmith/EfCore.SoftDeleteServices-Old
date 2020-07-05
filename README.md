# EfCore.SoftDeleteServices

This repo contains prototype code to handle

- **Simple soft delete**: where a single entity class can be hidden from normal queries and restore back if required
- **Cascade soft delete**: where when an entity is soft deleted, then its dependent entity classes are also soft deleted.

The code works, but there is a lot of work to turn this into a general NuGet library - see [ReleaseNotes.md](https://github.com/JonPSmith/EfCore.SoftDeleteServices/blob/master/ReleaseNotes.md) file for what needs to be done.

## Terms

- **Simple soft delete** mimics a simple, one row, hard delete. The entity/row is still in the database, but won't show up in EF Core query. But you can un-soft delete, referred to as a soft delete **reset**, and the makes the entity visible in an EF Core query.
- **Cascade soft delete** mimics the hard delete's cascade approach and will soft delete any dependant relationships (the EF Core `DeleteBehavior` has an effect on what happens).
- **Hard delete** is when you delete a row in the database, via the EF Core `Remove` method. A hard delete removes the row from the database and may effect other entities/rows.

## General information on how the simple and cascade methods work

There three basic things your can do
1. Set the soft/cascade delete property to hidden, i.e. soft deleted 
2. Reset the soft/cascade delete property to zero, i.e. it is now not soft deleted.
3. Find all the soft deleted items that are solft deleted and can be reset.

For the first two you either provide an entity you want to set/reset, or the primary key(s) of the entity to want to set/reset and it will find that entity (using `IgnoreQueryFilters` if needed) and then applies the set/reset.

Cascade soft delete has two other features to deal with hard deleting cascade soft deleted entries
- One to provide a "Are you sure..." message to show to the user before they hard delete something
- One to actually hard delete all the cascade entities had are already soft deleted.

The status the set/reset/hard delete method returns contains:
- `IsValid` is true if no errors where found.
- `Errors` List of errors - empty list if no errors.
- `Result` (int) returns 
   - *Success*: number of entity classes were set/reset/deleted etc. 
   - *Errors or Not Found*: will be 0. NOTE: if `notFoundAllowed` is true then you won't get an error on Not Found (useful for Web APIs).
- `Message` which provides a user-friendly message saying what has happened, for instance "Successfully soft deleted that entry". If errors then `Message` says "Failed with xx errors".


*See [GenericServices.StatusGeneric](https://github.com/JonPSmith/GenericServices.StatusGeneric) for more on how to use the returned `IStatusGeneric<int>` status in the best way. There is a library that can turn errors into ASP.NET Core ModelErrors or Web API returns.*

## Simple soft delete methods

The interface `ISoftDeleteService` contains all the methods you can use to soft delete a single entity class. The methods are:

- `SetSoftDeleteViaKeys<TEntity>`, which will find the `TEntity` class with the given primary or alternate keys and then sets its soft deleted flag.
- `ResetSoftDeleteViaKeys<TEntity>`, which will find the `TEntity` class with the given primary or alternate keys and then reset the soft deleted flag.
- `SetSoftDelete<TEntity>` takes in an entity class and sets its soft deleted flag.
- `ResetSoftDelete<TEntity>` takes in an entity class and resets its soft deleted flag.
- `GetSoftDeletedEntries<TEntity>()` this will return all the entities of type `TEntity` that are soft deleted.

See code/documentation for more infromation.


## Cascade soft delete methods

... still to write. Just look at the code for now as it has good comments

