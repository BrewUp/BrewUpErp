# Data Model: BrewApp Navigation Shell

This document describes the core entities and state objects involved in the navigation shell for BrewApp. It focuses on UI/navigation-related data rather than business domain data (which belongs to module-specific specifications).

## Entities

### 1. NavigationMenuItem

Represents a single entry in the navigation menu (e.g., MasterData, Sales).

**Fields**:
- `Id` (string): Unique identifier for the menu item (e.g., "masterdata", "sales").
- `DisplayName` (string): Localizable label shown to the user.
- `Icon` (string / IconSource): Reference to an icon resource.
- `Route` (string): Shell route associated with this item (e.g., "//modules/masterdata").
- `Order` (int): Display order in the menu.
- `IsEnabled` (bool): Whether the menu item is currently enabled/visible.

**Relationships**:
- Belongs to a `ModuleDescriptor` (1:1 logical mapping).

**Validation Rules**:
- `Id` MUST be non-empty and unique across all menu items.
- `Route` MUST correspond to a registered Shell route.
- `DisplayName` MUST be non-empty and localizable.

---

### 2. ModuleDescriptor

Defines high-level metadata for a functional module.

**Fields**:
- `Key` (string): Logical key for the module (e.g., "MasterData", "Sales").
- `Name` (string): Human-readable name for the module.
- `RoutePrefix` (string): Base route segment for module pages (e.g., "modules/masterdata").
- `PrimaryPageRoute` (string): Route to the module's landing/placeholder page.
- `Icon` (string / IconSource): Module icon reference.
- `IsEnabled` (bool): Whether the module is currently enabled.

**Relationships**:
- Associated with exactly one `NavigationMenuItem`.
- Module-specific pages and ViewModels (defined in module specs) will reference the module's `RoutePrefix`.

**Validation Rules**:
- `Key` MUST be unique across modules.
- `RoutePrefix` MUST be unique and valid as a Shell route segment.

---

### 3. AppShellState

Captures the current state of the Shell navigation.

**Fields**:
- `CurrentModuleKey` (string): Key of the currently active module (or `null` when on home/landing).
- `NavigationStack` (Stack<string>): Logical stack of route identifiers for back navigation.
- `IsFlyoutOpen` (bool): Whether the navigation menu is currently open.
- `LastVisitedModuleKey` (string): Key of the last visited module.

**Relationships**:
- References the set of `ModuleDescriptor` instances available in the app.

**Validation Rules**:
- `CurrentModuleKey` MUST be `null` or match a known `ModuleDescriptor.Key`.
- `NavigationStack` entries MUST correspond to registered routes.

---

### 4. LandingPageViewModel

ViewModel backing the landing page, used for MVVM bindings.

**Fields**:
- `Title` (string): App title / brewery name.
- `Subtitle` (string): Short description/tagline.
- `HeroImageSource` (string / ImageSource): Path or resource key for the landing image.
- `IsOffline` (bool): Indicates whether the app detects network/API unavailability.
- `Modules` (ObservableCollection<NavigationMenuItem>): List of modules to show as quick-access tiles (optional).

**Validation Rules**:
- `Title` MUST be non-empty.
- `HeroImageSource` MUST point to a valid image resource.

---

### 5. ModulePlaceholderViewModel

Generic ViewModel for module placeholder screens until real functionality is implemented.

**Fields**:
- `ModuleKey` (string): Key of the module this screen represents.
- `ModuleName` (string): Display name of the module.
- `Description` (string): Short text explaining that this is a placeholder.

**Validation Rules**:
- `ModuleKey` MUST map to a known `ModuleDescriptor`.

---

## State Transitions

### App Launch → Landing Page

1. App starts and initializes DI container and Shell.
2. `AppShellState.CurrentModuleKey` = null.
3. Landing page is displayed with hero image and navigation entry points.

### Landing Page → Module Screen

1. User selects a module from the flyout menu or landing tiles.
2. `AppShellState.CurrentModuleKey` is set to the selected module key.
3. Route navigation to the module's primary page is triggered using Shell navigation.
4. `AppShellState.NavigationStack` is updated with the new route.

### Module Screen → Another Module

1. User opens flyout menu from any module page.
2. User selects a different module.
3. Shell navigates to the new module's primary route.
4. `AppShellState.CurrentModuleKey` is updated accordingly.

### Module Screen → Home

1. User taps "Home" or app logo.
2. Shell navigates back to the landing page route.
3. `AppShellState.CurrentModuleKey` is set to null.

---

## Notes

- Business entities (e.g., Products, Orders, Inventory Movements) are out of scope here and will be defined in module-specific data models.
- This data model focuses on navigation and app shell, ensuring MVVM bindings are clear and testable.
