# Release Notes

## Release Notes

The code is not quite ready to be a library - see lists below.

However it is clean enough that you could copy the code and use in your application. NOTE: the interface most likely will change before it becomes a library

## Things needed to turn it into a library

### Single soft delete (95% done)

- **TODO**: Provide Async versions
- **TODO**: Need more unit tests
    - Check `callSaveChanges` usage
- DONE: ResetSoftDeleteViaKeys should filter out other query filters so only valid soft deleted entries reset
- DONE: Add HardDeleteSoftDeletedEntries to single soft delete
- DONE: Allow user to provide their own `ISoftDelete` interface (moderate)
- DONE: Allow user to define some message parts (moderate)
- DONE: GetSoftDeletedEntries contains other query filters so only valid soft deleted  entries are shown (hard)
- DONE: Allowing multiple soft delete, e.g. archive & soft delete

### Cascade soft delete (80% done)

- **TODO**: Have separate configuration class
- **TODO**: Change `CascadeWalker` to return the loaded relationships, with filtering, so that it works with properties, backing fields and shadow properties (hard)
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
