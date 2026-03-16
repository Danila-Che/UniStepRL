# Simple Environment Logic

## Action

A tower can only be built on an empty player hex.

Units can only be placed:
- on an empty player hex;
- on a player hex where units can merge into a single unit according to the merging rules;
- on a neutral neighbour hex;
- on a opponent neighbour hex if the unit's strength exceeds the maximum defense of neighbour enemy hexes.

If a unit is placed on a player hex, it can move.

Movement can only occur from a player hex containing units that can move.

Movement is only possible within movement range and according to unit placement logic. Once moved, a unit cannot move.

End of turn. Before each turn, the current player's units can move.

If, after a player's step, part of the enemy's territory is cut off, then all enemy units on the cut off part are killed and all towers are destroyed.

## Economics

After a player completes their turn, their unit upkeep is deducted. If their unit upkeep is less than zero, all of their units are destroyed.
After a player's turn, they are awarded gold equal to the amount of territory directly adjacent to their fort.

After each step, map cells is recalculated.

## Win Condition

Побеждает тот игрок, который первым поставил своего юнита не клетку с фортом потивника.

Can build tower only on player empty cell.

Can place unit only on player empty cell or player cell with unit that can merge with or neutral cell or if strength point of unit is high of max defence neighbor cells of oppenent.

Can move point only from player cell that contains movable unit.

Can move unit only in movement range and with place unit logic.

# Data

Movement Radius: 4.

Economy Table:

| Building/Unit | Cost | Maintenance |
|---------------|------|-------------|
| Tower         | 15   | 0           |
| Peasant       | 10   | 2           |
| Spearman      | 20   | 6           |
| Knight        | 30   | 18          |
| Baron         | 40   | 64          |

Available Merge Table:

|          | Peasant | Spearman | Knight | Baron |
|----------|---------|----------|--------|-------|
| Peasant  | +       | +        | +      | -     |
| Spearman | +       | +        | -      | -     |
| Knight   | +       | -        | -      | -     |
| Baron    | -       | -        | -      | -     |

Merge Result Table:

|          | Peasant  | Spearman | Knight | Baron |
|----------|----------|----------|--------|-------|
| Peasant  | Spearman | Knight   | Baron  | -     |
| Spearman | Knight   | Baron    | -      | -     |
| Knight   | Baron    | -        | -      | -     |
| Baron    | -        | -        | -      | -     |

Strength Point Table:

| Unit     | Point |
|----------|-------|
| Peasant  | 1     |
| Spearman | 2     |
| Knight   | 3     |
| Baron    | 4     |

Defence Point Table:

| Building/Unit | Point |
|---------------|-------|
| Fort          | 1     |
| Tower         | 2     |
| Peasant       | 1     |
| Spearman      | 2     |
| Knight        | 3     |
| Baron         | 4     |
