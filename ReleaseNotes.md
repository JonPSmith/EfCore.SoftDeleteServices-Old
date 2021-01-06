# Release Notes

## Release Notes

The code is not quite ready to be a library - see lists below.

However it is clean enough that you could copy the code and use in your application. NOTE: the interface most likely will change before it becomes a library

## Things needed to turn it into a library

### Single soft delete (95% done)

- **TODO**: Provide Async versions (moderate)
- DONE: ResetSoftDeleteViaKeys should filter out other query filters so only valid soft deleted entries reset
- DONE: Add HardDeleteSoftDeletedEntries to single soft delete
- DONE: Allow user to provide their own `ISoftDelete` interface (moderate)
- DONE: Allow user to define some message parts (moderate)
- DONE: GetSoftDeletedEntries contains other query filters so only valid soft deleted  entries are shown (hard)
- DONE: Allowing multiple soft delete, e.g. archive & soft delete

### Cascade soft delete (90% done)

Limitation: The hard delete will return the wrong number if
1. The dependents have a delete behavior of CascadeDelete or ClientCascadeDelete AND...
2. One or more of the dependent entities have a multi-tenant query filter like UserId 

That's because the cascade delete service can't see any entries but the cascade delete WILL delete it. This isn't likely, but  

- **TODO**: Provide Async versions (moderate)
- DONE: Change `CascadeWalker` to return the loaded relationships, with filtering,  (hard)
- DONE: ResetSoftDeleteViaKeys should filter out other query filters so only valid soft deleted entries reset (moderate)
- DONE: Move `ReadEveryTime` to config
- DONE: Handle other query filter parts (hard)
- DONE: Add versions of the methods that find the starting entity via primary keys (easy)
- DONE: Use GenericServices's approach to not found (moderate)
- DONE: Allow user to provide their own `ICascadeSoftDelete` interface (moderate)
- DONE: Allow user to define some message parts (moderate)


### Other things

* Example of using shadow properties for cascade delete
* Documentation
* Think about using SQL code to improve cascade soft delete performance (very hard)

### Possible improvements

- Change so that it works with navigational properties, backing fields and shadow properties (easy, but needs lots of unit tests)

