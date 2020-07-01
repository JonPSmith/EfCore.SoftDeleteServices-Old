# EfCore.SoftDeleteServices

This repo contains prototype code to handle

- **Simple soft delete**: where a single entity class can be hidden from normal queries and restore back if required
- **Cascade soft delete**: where when an entity is soft deleted, then its dependent entity classes are also soft deleted.

The code works, but there is a lot of work to turn this into a general NuGet library - see ReleasNotes.md file for what needs to be done.

## Terms

- **Hard delete** is when you delete a row in the database, via the EF Core `Remove` method. A hard delete removes the row from the database and may effect other entities/rows.
- **Simple soft delete** mimics a simple, one row, hard delete but the entity/row is still there, but won't show up in EF Core query. But you can un-soft delete, referred to as a soft delete **reset**, and the makes the entity visible in an EF Core query.
- **Cascade soft delete** mimics the hard delete's cascade approach and will soft delete any dependant relationships (the EF Core `DeleteBehavior` has an effect on what happens).

