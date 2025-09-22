# SaveFramework.Components

[← Back to Home](Home.md)

## SaveId

**Description:** Component that provides a unique identifier for save/load operations

### Methods and Properties

#### GenerateNewId (Method) {#saveid-generatenewid-method}
```csharp
void GenerateNewId()
```


**Description:** Generate a new unique identifier

#### GetDisplayName (Method) {#saveid-getdisplayname-method}
```csharp
string GetDisplayName()
```


**Description:** Get a display name for this [SaveId](SaveFramework-Components.md#saveid) (useful for debugging)

#### HasValidId (Method) {#saveid-hasvalidid-method}
```csharp
bool HasValidId()
```


**Description:** Check if this [SaveId](SaveFramework-Components.md#saveid) has a valid identifier

#### SetCustomId (Method) {#saveid-setcustomid-method}
```csharp
void SetCustomId(string customId)
```


**Description:** Set a custom ID (useful for predictable IDs in some cases)

---

*Last updated: 2025-09-22 09:29:54 UTC*
