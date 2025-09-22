# SaveFramework.Components

[← Back to Home](Home.md)

## SaveId

**Description:** Component that provides a unique identifier for save/load operations

### Methods and Properties

#### GenerateNewId (Method)
```csharp
void GenerateNewId()
```


**Description:** Generate a new unique identifier

#### GetDisplayName (Method)
```csharp
string GetDisplayName()
```


**Description:** Get a display name for this SaveId (useful for debugging)

#### HasValidId (Method)
```csharp
bool HasValidId()
```


**Description:** Check if this SaveId has a valid identifier

#### SetCustomId (Method)
```csharp
void SetCustomId(string customId)
```


**Description:** Set a custom ID (useful for predictable IDs in some cases)

---

*Last updated: 2025-09-22 09:29:54 UTC*
