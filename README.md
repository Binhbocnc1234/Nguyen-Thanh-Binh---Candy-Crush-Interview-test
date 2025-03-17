# **README**

## **Removed Features**
### **Board**
- `Shuffle()`
- `Swap()`
- `ShiftDownItems()`
- `FillGapsWithNewItems()`
- `GetHorizontalMatches()`
- `GetVerticalMatches()`

### **Board Controller**
- `FindMatchesAndCollapse()`
- `ShowHint()`
- `Update()` (old version)
- `CollapseMatches()`
- `ShuffleBoardCoroutine()`
- `ShiftDownItemsCoroutine()`

### **Level Moves**
- Previously, the condition for `eGameState::GameOver` was all **16 moves** had to be made. Now, this condition is **removed**.
- The **new condition** for **Game Over** is when the **Backpack is full**.

---

## **Added Features**
### **Singleton**
- Added `Singleton.cs` to the **Utils** folder.
- Some classes now use the **Singleton** pattern:
  - `GameManager`
  - `Backpack`
  - `BoardController`

### **Cell**
- **New Variables:**
  - `isInteractable`: Some cells in `BottomBoard` are overlapped by cells in `UpperBoard`, so the player **cannot** click on them. `true` if the player **can** click, otherwise `false`.
  - `countOverlapped` (private set): Keeps track of how many **UpperBoard** cells overlap this cell.
- **New Functions:**
  - `ToggleInteractable(bool value)`: Sets the value of `isInteractable`. If `false`, it will **darken the renderer color**.
  - `AddOverlapped(int offset)`: Modifies the value of `countOverlapped`.

### **Backpack** *(MonoBehaviour)*
- **`Start()`**: Constructs **5 empty Cells** horizontally.
- **`AddToBackpack(Cell c)`**: Moves the item from a `Cell` into an **available Cell** in `List<Item>`, using **DOTween** for smooth movement.
- **`FindMatchesAndCollapse()`**: If there are **3 identical Items** in the **Backpack**, then **delete those 3 Items**, using **DOTween** to shorten `List<Item>`.

### **Board**
- **Refactored `Fill()`** into `FillWithRandomItem()`.
- **Modified `FillWithRandomItem()`** so that the total of each Item is **divisible by 3**.

### **Board Controller**
- `m_board` now becomes **upperBoard** and **lowerBoard**.
- Became a **partial class**: split into two scripts(main logic and AutoPlay mode).
- **New Functions:**
  - `ProcessClick()`: When the player clicks on a **Cell** containing an **Item (`Item != null`)**, execute `AddToBackpack(Cell)` from **Backpack** (disabled in **AutoPlayMode**).
  - `IEnumerator AutoPlayRoutine()`: Activated in **AutoPlayMode**.
  - `StartGame()`: Added **BottomBoard**.
  - `Update()`: Now receives **signals from `Input.MouseButtonDown(0)`**.

### **GameManager**
- New `eLevelMode` Value: `NormalMode`.
- New `eStateGame` Value: `Win`.
- **New Function:**
  - `Win()`: Happens when both `bottom_board` and `upper_board` are cleared.

### **UPanelWin**
- Deployed from **`IMenu`**.
- **Displays when `eStateGame == Win`**.

---

This document details all recent changes, including **removed features**, **new mechanics**, and **refactored functions**. If you have any questions, feel free to ask me: binhbocnc1234@gmail.com!

