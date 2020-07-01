# Release Notes

## Release Notes

The code is currently a prototype and not ready to be a library.

## Things needed to turn it into a library

### Simple soft delete

* Allow user to provide their own `ISoftDelete` interface (moderate)
* Allow user to define some message parts (moderate)
* Handle other query filter parts (hard)
* Needs comments
* Needs a lot more unit tests

### Cascade soft delete

* Add versions of the methods that find the starting entity via primary keys (easy)
* Use GenericServices's approach to not found (moderate)
* Allow user to provide their own `ICascadeSoftDelete` interface (moderate)
* Allow user to define some message parts (moderate)
* Change `CascadeWalker` to return the loaded relationships so that it works with properties, backing fields and shadow properties (easy)
* Handle other query filter parts (hard)
* Think about using SQL code to improve performance (very hard)

### Other things

* Add Auto configuring helpers
* Show how it can work with multi-tenant
* Documentation
