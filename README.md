# ROC Code Style Guide

This document outlines the coding standards and style guidelines for the ROC project.

## Naming Conventions

- **Private Fields**: Start with underscore (`_camelCase`)
- **Public Fields**: Use camelCase (no underscore)
- **Properties**: Use PascalCase
- **Methods**: Use PascalCase
- **Classes/Interfaces**: Use PascalCase
- **Namespaces**: Use PascalCase
- **Events**: Use PascalCase with "Event" suffix
- **Interfaces**: Start with "I" prefix
- **Enums**: Use PascalCase, singular form

## File Organization

- One class per file (exceptions for small related classes)
- Filename should match the primary class/interface name
- Group related files in appropriate directories
- Use meaningful namespace hierarchies that reflect directory structure

## Code Structure

### Class Structure:
1. Private fields
2. Public fields
3. Properties
4. Constructors/Initialization
5. Public methods
6. Protected methods
7. Private methods
8. Nested types

### Regions:
- Avoid excessive use of #region directives
- If used, regions should group functional areas, not access modifiers

## Formatting

- Use tabs for indentation (4 spaces equivalent)
- Place braces on new lines for methods and classes
- Place braces on the same line for properties, lambdas, and control structures
- Use spaces around operators
- Limit line length to 120 characters when possible


## SOLID Principles

- Follow Single Responsibility Principle
- Follow Open/Closed Principle
- Follow Liskov Substitution Principle
- Follow Interface Segregation Principle
- Follow Dependency Inversion Principle

## Dependency Injection

- Use constructor injection for required dependencies
- Don't inject fields directly, only use Construct methods
- Mark optional dependencies as such

## Resource Management

- Implement IDisposable for classes that manage resources
- Use `using` statements or try-finally blocks to ensure proper cleanup
- Dispose of resources in the Dispose method
- Check for null before disposing resources

## Error Handling

- Use exceptions for exceptional conditions only
- Prefer validation over catching exceptions when possible
- Include context information in exception messages
- Log exceptions with appropriate severity
- Clean up resources in exception handling blocks

## Unity-Specific Guidelines

- Minimize use of MonoBehaviour
- Prefer ScriptableObjects for configuration
- Use UniTask for asynchronous operations
- Use Addressables for asset management
- Use VContainer for dependency injection
- Implement null checks for Unity references
- Implement initialization validation

## Anti-Patterns to Avoid

- Singletons (use dependency injection instead)
- Service Locator pattern (use dependency injection instead)
- Classes named "Manager" or "Controller" (use more specific names)
- God objects (break down large classes into smaller, focused ones)
- Deep inheritance hierarchies (prefer composition over inheritance)
- Public fields without proper encapsulation
- Excessive comments (write self-documenting code)
- Magic numbers (use constants or configuration values) 
- Backing fields (Use auto-properties with private setters)