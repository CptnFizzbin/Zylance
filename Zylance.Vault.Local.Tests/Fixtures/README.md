# Test Fixtures

This directory contains SQLite database files used for testing the Zylance vault functionality.

## Planned Test Databases

The following database files will be created in the future for comprehensive testing:

- **empty.zlv.sqlite** - An empty SQLite database file for testing empty database handling
- **non-zylance.zlv.sqlite** - A valid SQLite database without the `_zylance_` marker table, used to test that non-Zylance databases are properly rejected
- **zylance.zlv.sqlite** - A valid Zylance vault database with the `_zylance_` marker table, used to test opening existing Zylance vaults

## Current Testing Approach

Currently, the `MarkerTableTests` class creates temporary database files during test execution to validate:
- Creation of the `_zylance_` marker table in new databases
- Opening existing Zylance databases successfully
- Rejection of non-Zylance databases with appropriate exceptions
- Metadata storage in the marker table
- Correct schema for the marker table

These temporary files are created in the system's temp directory and cleaned up after each test run.
