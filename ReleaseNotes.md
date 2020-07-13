# Release Notes

## Release Notes

The code is currently a prototype and not ready to be a library.

However it is clean enough that you could cop the code and use in your application. NOTE: the interface most likely will change before it becomes a library

## Things needed to turn it into a library

### Single soft delete (90% done)

- **TODO**: ResetSoftDeleteViaKeys should filter out other query filters so only valid soft deleted entries reset
- **TODO**: Provide Async versions
- **TODO**: Add HardDeleteSoftDeletedEntries to single soft delete
- **TODO**: Need more unit tests
    - Check ResetSoftDeleteViaKeys doesn't reset entries it shouldn't be able to get to
    - Check `callSaveChanges` usage
- DONE: Allow user to provide their own `ISoftDelete` interface (moderate)
- DONE: Allow user to define some message parts (moderate)
- DONE: GetSoftDeletedEntries contains other query filters so only valid soft deleted  entries are shown (hard)
- DONE: Allowing multiple soft delete, e.g. archive & soft delete

### Cascade soft delete (60% done)

- **TODO**: Handle other query filter parts (hard)
- **TODO**: Change `CascadeWalker` to return the loaded relationships, with filtering, so that it works with properties, backing fields and shadow properties (hard)
- **TODO**: ResetSoftDeleteViaKeys should filter out other query filters so only valid soft deleted entries reset (moderate)
- **TODO**: Move `ReadEveryTime` to config
- **TODO**: Think about using SQL code to improve performance (very hard)
- DONE: Add versions of the methods that find the starting entity via primary keys (easy)
- DONE: Use GenericServices's approach to not found (moderate)
- DONE: Allow user to provide their own `ICascadeSoftDelete` interface (moderate)
- DONE: Allow user to define some message parts (moderate)


### Other things

* Example of using shadow properties for cascade delete
* Documentation
