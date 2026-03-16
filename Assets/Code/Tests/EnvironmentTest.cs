// using System;
// using System.Linq;
// using NUnit.Framework;
// using UniStepRL;
// using UnityEditor.VersionControl;
// using Environment = UniStepRL.Environment;

// namespace Tests
// {
//     public class EnvironmentTest
//     {
//         [Test]
//         [TestCase(0, 0, 0, 0, 0)]
//         [TestCase(0, 0, 1, 0, 1)]
//         [TestCase(0, 0, 0, 1, 1)]
//         [TestCase(0, 0, 2, 0, 2)]
//         [TestCase(0, 0, 1, 1, 2)]
//         [TestCase(0, 0, 3, 0, 3)]
//         [TestCase(0, 0, 2, 1, 3)]
//         [TestCase(0, 0, 4, 0, 4)]
//         [TestCase(0, 0, 3, 1, 4)]
//         [TestCase(0, 0, 5, 0, 5)]
//         [TestCase(0, 0, 4, 1, 5)]
//         public void CalculateDistance(int colA, int rowA, int colB, int rowB, int distance)
//         {
//             var a = new OffsetCoordinates(colA, rowA);
//             var b = new OffsetCoordinates(colB, rowB);

//             var cubeA = a.OddRToCube();
//             var cubeB = b.OddRToCube();
            
//             Assert.That(CubeCoordinates.Distance(cubeA, cubeB), Is.EqualTo(distance));
//         }
        
//         [Test]
//         [TestCase(5, 2, 9)]
//         [TestCase(2, 5, 8)]
//         [TestCase(5, 5, 23)]
//         public void CreateEnvironment(int width, int height, int expectedFlatSize)
//         {
//             var env = new Environment(width, height);

//             Assert.That(env.CellCount, Is.EqualTo(expectedFlatSize));
//         }

//         [Test]
//         public void InitializeEnvironment()
//         {
//             var env = new Environment(width: 5, height: 5);

//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
//             env.CreateBase(new OffsetCoordinates(4, 4), Player.Player2);

//             Assert.That(env.GetUnit(new OffsetCoordinates(0, 0)), Is.EqualTo(Unit.Fort));
//             Assert.That(env.GetUnit(new OffsetCoordinates(1, 0)), Is.EqualTo(Unit.None));
//             Assert.That(env.GetUnit(new OffsetCoordinates(0, 1)), Is.EqualTo(Unit.None));
//             Assert.That(env.GetPlayer(new OffsetCoordinates(0, 0)), Is.EqualTo(Player.Player1));
//             Assert.That(env.GetPlayer(new OffsetCoordinates(1, 0)), Is.EqualTo(Player.Player1));
//             Assert.That(env.GetPlayer(new OffsetCoordinates(0, 1)), Is.EqualTo(Player.Player1));
            
//             Assert.That(env.GetUnit(new OffsetCoordinates(4, 4)), Is.EqualTo(Unit.Fort));
//             Assert.That(env.GetUnit(new OffsetCoordinates(3, 4)), Is.EqualTo(Unit.None));
//             Assert.That(env.GetUnit(new OffsetCoordinates(3, 3)), Is.EqualTo(Unit.None));
//             Assert.That(env.GetPlayer(new OffsetCoordinates(4, 4)), Is.EqualTo(Player.Player2));
//             Assert.That(env.GetPlayer(new OffsetCoordinates(3, 4)), Is.EqualTo(Player.Player2));
//             Assert.That(env.GetPlayer(new OffsetCoordinates(3, 3)), Is.EqualTo(Player.Player2));
            
//             Assert.That(env.GetUnit(new OffsetCoordinates(2, 2)), Is.EqualTo(Unit.None));
//             Assert.That(env.GetPlayer(new OffsetCoordinates(2, 2)), Is.EqualTo(Player.None));
//         }

//         [Test]
//         public void EndTurn()
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             Assert.That(env.CurrentPlayer, Is.EqualTo(Player.Player1));

//             env.Step(); // end turn
            
//             Assert.That(env.CurrentPlayer, Is.EqualTo(Player.Player2));
//         }

//         [Test]
//         public void UpdateplayerState()
//         {
//             var env = new Environment(width: 5, height: 5);

//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
            
//             Assert.That(env.GetPlayerState(Player.Player1).Amount, Is.EqualTo(10));

//             env.Step(); // end turn
            
//             Assert.That(env.GetPlayerState(Player.Player1).Amount, Is.EqualTo(13));
//         }

//         #region Build
        
//         [Test]
//         public void BuildTower()
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);

//             // An empty cell of the "land" type owned by the player.
            
//             env.Build(new OffsetCoordinates(1, 0), Unit.Tower);
//             env.Build(new OffsetCoordinates(0, 1), Unit.Tower);
            
//             Assert.That(env.GetUnit(new OffsetCoordinates(1, 0)), Is.EqualTo(Unit.Tower));
//             Assert.That(env.GetUnit(new OffsetCoordinates(0, 1)), Is.EqualTo(Unit.Tower));
//         }

//         [Test]
//         public void ForbidBuildingTowerOnFort()
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);

//             Assert.Throws<InvalidOperationException>(() => env.Build(new OffsetCoordinates(0, 0), Unit.Tower));
//         }
        
//         [Test]
//         public void ForbidBuildingTowerOnNotPlayerCell()
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);

//             Assert.Throws<InvalidOperationException>(() => env.Build(new OffsetCoordinates(2, 2), Unit.Tower));
//         }
        
//         [Test]
//         public void ForbidBuildingTowerOnWaterCell()
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
//             env.SetCell(new OffsetCoordinates(1, 0), Cell.Water);

//             Assert.Throws<InvalidOperationException>(() => env.Build(new OffsetCoordinates(1, 0), Unit.Tower));
//         }
        
//         [Test]
//         public void ForbidBuildingTowerOnTower()
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);

//             env.Build(new OffsetCoordinates(1, 0), Unit.Tower);
            
//             Assert.Throws<InvalidOperationException>(() => env.Build(new OffsetCoordinates(1, 0), Unit.Tower));
//         }
        
//         [Test]
//         public void ForbidBuildingTowerOnUnit()
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
        
//             env.Place(new OffsetCoordinates(1, 0), Unit.Peasant);
            
//             Assert.Throws<InvalidOperationException>(() => env.Build(new OffsetCoordinates(1, 0), Unit.Tower));
//         }
        
//         #endregion

//         #region Place

//         [Test]
//         [TestCase(Unit.Peasant)]
//         [TestCase(Unit.Spearman)]
//         [TestCase(Unit.Knight)]
//         [TestCase(Unit.Baron)]
//         public void PlaceEntityOnEmptyPlayerLandCell(Unit entity)
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);

//             // An empty cell of the "land" type owned by the player.
//             // Can merge units.
//             // Occupy neutral cell.
//             // If the cell belongs to the enemy, an attack can only be made if the cell's defense is lower than the unit's strength.
            
//             env.Place(new OffsetCoordinates(1, 0), entity);
            
//             Assert.That(env.GetUnit(new OffsetCoordinates(1, 0)), Is.EqualTo(entity));
//         }
        
//         [Test]
//         public void ForbidPlacingEntityOnWaterCell()
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
//             env.SetCell(new OffsetCoordinates(1, 0), Cell.Water);

//             Assert.Throws<InvalidOperationException>(() => env.Place(new OffsetCoordinates(1, 0), Unit.Peasant));
//         }
        
//         [Test]
//         public void ForbidPlacingEntityOnFort()
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);

//             Assert.Throws<InvalidOperationException>(() => env.Place(new OffsetCoordinates(0, 0), Unit.Peasant));
//         }
        
//         [Test]
//         public void ForbidPlacingEntityOnTower()
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
            
//             env.Build(new OffsetCoordinates(1, 0), Unit.Tower);

//             Assert.Throws<InvalidOperationException>(() => env.Place(new OffsetCoordinates(1, 0), Unit.Peasant));
//         }

//         [Test]
//         [TestCase(Unit.Peasant, Unit.Peasant, Unit.Spearman)]
//         [TestCase(Unit.Peasant, Unit.Spearman, Unit.Knight)]
//         [TestCase(Unit.Peasant, Unit.Knight, Unit.Baron)]
//         [TestCase(Unit.Spearman, Unit.Peasant, Unit.Knight)]
//         [TestCase(Unit.Spearman, Unit.Spearman, Unit.Baron)]
//         [TestCase(Unit.Knight, Unit.Peasant, Unit.Baron)]
//         public void PlaceEntityOnEntity(Unit entity0, Unit entity1, Unit result)
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
            
//             env.Place(new OffsetCoordinates(1, 0), entity0);
//             env.Place(new OffsetCoordinates(1, 0), entity1);
            
//             Assert.That(env.GetUnit(new OffsetCoordinates(1, 0)), Is.EqualTo(result));
//         }

//         [Test]
//         [TestCase(Unit.Peasant, Unit.Baron)]
//         [TestCase(Unit.Spearman, Unit.Knight)]
//         [TestCase(Unit.Spearman, Unit.Baron)]
//         [TestCase(Unit.Knight, Unit.Spearman)]
//         [TestCase(Unit.Knight, Unit.Knight)]
//         [TestCase(Unit.Knight, Unit.Baron)]
//         [TestCase(Unit.Baron, Unit.Peasant)]
//         [TestCase(Unit.Baron, Unit.Spearman)]
//         [TestCase(Unit.Baron, Unit.Knight)]
//         [TestCase(Unit.Baron, Unit.Baron)]
//         public void ForbidPlacingEntityOnEntityIfItCannotBeMerged(Unit entity0, Unit entity1)
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
            
//             env.Place(new OffsetCoordinates(1, 0), entity0);
            
//             Assert.Throws<InvalidOperationException>(() => env.Place(new OffsetCoordinates(1, 0), entity1));
//         }

//         [Test]
//         public void OccupyNeutralFringesCell()
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
            
//             Assert.That(env.GetPlayer(new OffsetCoordinates(2, 0)), Is.EqualTo(Player.None));
            
//             env.Place(new OffsetCoordinates(2, 0), Unit.Peasant);
            
//             Assert.That(env.GetUnit(new OffsetCoordinates(2, 0)), Is.EqualTo(Unit.Peasant));
//             Assert.That(env.GetPlayer(new OffsetCoordinates(2, 0)), Is.EqualTo(Player.Player1));
//         }

//         [Test]
//         public void ForbidPlacingEntityOnNotPlayerOrNotFringesCell()
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
            
//             Assert.Throws<InvalidOperationException>(() => env.Place(new OffsetCoordinates(3, 0), Unit.Peasant));
//         }

//         [Test]
//         public void AttackOpponentEmptyCell()
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
//             env.CreateBase(new OffsetCoordinates(3, 0), Player.Player2);
            
//             Assert.That(env.GetPlayer(new OffsetCoordinates(2, 0)), Is.EqualTo(Player.Player2));
            
//             env.Place(new OffsetCoordinates(2, 0), Unit.Spearman);
            
//             Assert.That(env.GetPlayer(new OffsetCoordinates(2, 0)), Is.EqualTo(Player.Player1));
//         }
        
//         [Test]
//         [TestCase(Unit.Spearman, Unit.Peasant)]
//         [TestCase(Unit.Knight, Unit.Peasant)]
//         [TestCase(Unit.Knight, Unit.Spearman)]
//         [TestCase(Unit.Baron, Unit.Peasant)]
//         [TestCase(Unit.Baron, Unit.Spearman)]
//         [TestCase(Unit.Baron, Unit.Knight)]
//         [TestCase(Unit.Baron, Unit.Baron)]
//         public void AttackOpponentEntity(Unit playerEntity, Unit opponentEntity)
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
//             env.CreateBase(new OffsetCoordinates(3, 0), Player.Player2);
            
//             env.SetPlayer(Player.Player2);
//             env.Place(new OffsetCoordinates(2, 0), opponentEntity);
//             Assert.That(env.GetPlayer(new OffsetCoordinates(2, 0)), Is.EqualTo(Player.Player2));
//             Assert.That(env.GetUnit(new OffsetCoordinates(2, 0)), Is.EqualTo(opponentEntity));
            
//             env.SetPlayer(Player.Player1);
//             env.Place(new OffsetCoordinates(2, 0), playerEntity);
            
//             Assert.That(env.GetPlayer(new OffsetCoordinates(2, 0)), Is.EqualTo(Player.Player1));
//             Assert.That(env.GetUnit(new OffsetCoordinates(2, 0)), Is.EqualTo(playerEntity));
//         }
        
//         [Test]
//         [TestCase(Unit.Peasant, Unit.Peasant)]
//         [TestCase(Unit.Peasant, Unit.Spearman)]
//         [TestCase(Unit.Peasant, Unit.Knight)]
//         [TestCase(Unit.Peasant, Unit.Baron)]
//         [TestCase(Unit.Spearman, Unit.Spearman)]
//         [TestCase(Unit.Spearman, Unit.Knight)]
//         [TestCase(Unit.Spearman, Unit.Baron)]
//         [TestCase(Unit.Knight, Unit.Knight)]
//         [TestCase(Unit.Knight, Unit.Baron)]
//         public void ForbidAttackOpponentEntity(Unit playerEntity, Unit opponentEntity)
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
//             env.CreateBase(new OffsetCoordinates(3, 0), Player.Player2);
            
//             env.SetPlayer(Player.Player2);
//             env.Place(new OffsetCoordinates(2, 0), opponentEntity);
//             Assert.That(env.GetPlayer(new OffsetCoordinates(2, 0)), Is.EqualTo(Player.Player2));
//             Assert.That(env.GetUnit(new OffsetCoordinates(2, 0)), Is.EqualTo(opponentEntity));
            
//             env.SetPlayer(Player.Player1);
            
//             Assert.Throws<InvalidOperationException>(() => env.Place(new OffsetCoordinates(2, 0), playerEntity));
//         }

//         [Test]
//         [TestCase(Unit.Knight)]
//         [TestCase(Unit.Baron)]
//         public void AttackOpponentTower(Unit playerEntity)
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
//             env.CreateBase(new OffsetCoordinates(3, 0), Player.Player2);
            
//             env.SetPlayer(Player.Player2);
//             env.Build(new OffsetCoordinates(2, 0), Unit.Tower);
//             Assert.That(env.GetPlayer(new OffsetCoordinates(2, 0)), Is.EqualTo(Player.Player2));
//             Assert.That(env.GetUnit(new OffsetCoordinates(2, 0)), Is.EqualTo(Unit.Tower));
            
//             env.SetPlayer(Player.Player1);
//             env.Place(new OffsetCoordinates(2, 0), playerEntity);
            
//             Assert.That(env.GetPlayer(new OffsetCoordinates(2, 0)), Is.EqualTo(Player.Player1));
//             Assert.That(env.GetUnit(new OffsetCoordinates(2, 0)), Is.EqualTo(playerEntity));
//         }
        
//         [Test]
//         [TestCase(Unit.Peasant)]
//         [TestCase(Unit.Spearman)]
//         public void ForbidAttackOpponentTower(Unit playerEntity)
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
//             env.CreateBase(new OffsetCoordinates(3, 0), Player.Player2);
            
//             env.SetPlayer(Player.Player2);
//             env.Build(new OffsetCoordinates(2, 0), Unit.Tower);
//             Assert.That(env.GetPlayer(new OffsetCoordinates(2, 0)), Is.EqualTo(Player.Player2));
//             Assert.That(env.GetUnit(new OffsetCoordinates(2, 0)), Is.EqualTo(Unit.Tower));
            
//             env.SetPlayer(Player.Player1);
            
//             Assert.Throws<InvalidOperationException>(() => env.Place(new OffsetCoordinates(2, 0), playerEntity));
//         }
        
//         [Test]
//         [TestCase(Unit.Spearman)]
//         [TestCase(Unit.Knight)]
//         [TestCase(Unit.Baron)]
//         public void AttackOpponentFort(Unit playerEntity)
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
//             env.CreateBase(new OffsetCoordinates(3, 0), Player.Player2);
            
//             Assert.That(env.GetPlayer(new OffsetCoordinates(2, 0)), Is.EqualTo(Player.Player2));
            
//             env.Place(new OffsetCoordinates(2, 0), Unit.Spearman);
//             env.Place(new OffsetCoordinates(3, 0), playerEntity);
            
//             Assert.That(env.GetPlayer(new OffsetCoordinates(3, 0)), Is.EqualTo(Player.Player1));
//             Assert.That(env.GetUnit(new OffsetCoordinates(3, 0)), Is.EqualTo(playerEntity));
//         }
        
//         [Test]
//         [TestCase(Unit.Peasant)]
//         public void ForbidAttackOpponentFort(Unit playerEntity)
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
//             env.CreateBase(new OffsetCoordinates(3, 0), Player.Player2);
            
//             Assert.That(env.GetPlayer(new OffsetCoordinates(2, 0)), Is.EqualTo(Player.Player2));
            
//             env.Place(new OffsetCoordinates(2, 0), Unit.Spearman);
            
//             Assert.Throws<InvalidOperationException>(() => env.Place(new OffsetCoordinates(3, 0), playerEntity));
//         }
        
//         [Test]
//         [TestCase(Unit.Knight)]
//         [TestCase(Unit.Baron)]
//         public void AttackOnOpponentCellIfItIsDefendedByTower(Unit playerEntity)
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
//             env.CreateBase(new OffsetCoordinates(3, 0), Player.Player2);
            
//             env.SetPlayer(Player.Player2);
//             env.Build(new OffsetCoordinates(2, 1), Unit.Tower);
            
//             env.SetPlayer(Player.Player1);

//             env.Place(new OffsetCoordinates(2, 0), playerEntity);
            
//             Assert.That(env.GetPlayer(new OffsetCoordinates(2, 0)), Is.EqualTo(Player.Player1));
//             Assert.That(env.GetUnit(new OffsetCoordinates(2, 0)), Is.EqualTo(playerEntity));
//         }
        
//         [Test]
//         public void ForbidAttackOnOpponentCellIfItIsDefendedByTower()
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
//             env.CreateBase(new OffsetCoordinates(3, 0), Player.Player2);
            
//             env.SetPlayer(Player.Player2);
//             env.Build(new OffsetCoordinates(2, 1), Unit.Tower);
            
//             env.SetPlayer(Player.Player1);
            
//             Assert.Throws<InvalidOperationException>(() => env.Place(new OffsetCoordinates(2, 0), Unit.Peasant));
//         }
        
//         #endregion
        
//         #region Move
        
//         [Test]
//         public void MoveEntityFromPlayerCellToPlayerCell()
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
            
//             env.Place(new OffsetCoordinates(1, 0), Unit.Peasant);
//             env.Move(new OffsetCoordinates(1, 0), new OffsetCoordinates(0, 1));
            
//             Assert.That(env.GetUnit(new OffsetCoordinates(1, 0)), Is.EqualTo(Unit.None));
//             Assert.That(env.GetUnit(new OffsetCoordinates(0, 1)), Is.EqualTo(Unit.Peasant));
//         }
        
//         [Test]
//         public void ForbidMoveToFar()
//         {
//             var env = new Environment(width: 5, height: 5);
            
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
            
//             env.Place(new OffsetCoordinates(1, 0), Unit.Peasant);
            
//             Assert.Throws<InvalidOperationException>(() =>
//                 env.Move(new OffsetCoordinates(1, 0), new OffsetCoordinates(4, 4)));
//         }
        
//         // TODO: More tests for movement action
        
//         #endregion

//         #region Old RL

//         [Test]
//         public void GetActionTypeMask()
//         {
//             var env = new Environment(width: 5, height: 5);
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);

//             var playerState = env.GetPlayerState(Player.Player1);
//             playerState.Amount = 0;

//             var mask = env.GetActionTypeMask();
//             Assert.That(mask, Is.EqualTo(new[] { 0, 0, 0, 1 }));
//         }
        
//         [Test]
//         public void GetActionTypeMask_CanPlaceEntity()
//         {
//             var env = new Environment(width: 5, height: 5);
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);

//             var playerState = env.GetPlayerState(Player.Player1);
//             playerState.Amount = 10;

//             var mask = env.GetActionTypeMask();
//             Assert.That(mask, Is.EqualTo(new[] { 0, 1, 0, 1 }));
//         }
        
//         [Test]
//         public void GetActionTypeMask_CanBuildTower()
//         {
//             var env = new Environment(width: 5, height: 5);
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);

//             var playerState = env.GetPlayerState(Player.Player1);
//             playerState.Amount = 15;

//             var mask = env.GetActionTypeMask();
//             Assert.That(mask, Is.EqualTo(new[] { 1, 1, 0, 1 }));
//         }
        
//         [Test]
//         public void GetActionTypeMask_CanMoveEntity()
//         {
//             var env = new Environment(width: 5, height: 5);
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);

//             var playerState = env.GetPlayerState(Player.Player1);
//             playerState.Amount = 0;
            
//             env.Place(new OffsetCoordinates(1, 0), Unit.Peasant);
            
//             var mask = env.GetActionTypeMask();
//             Assert.That(mask, Is.EqualTo(new[] { 0, 0, 1, 1 }));
//         }

//         [Test]
//         [TestCase(10, false)]
//         [TestCase(15, true)]
//         public void GetAvailableBuildingMask(int amount, bool canBuild)
//         {
//             var env = new Environment(width: 5, height: 5);
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);

//             var playerState = env.GetPlayerState(Player.Player1);
//             playerState.Amount = amount;

//             var mask = env.GetAvailableBuildingMask();
//             Assert.That(mask, Is.EqualTo(new[] { canBuild ? 1 : 0 }));
//         }

//         [Test]
//         public void GetAvailableBuildingCoordinatesMask()
//         {
//             var env = new Environment(width: 5, height: 5);
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
//             env.CreateBase(new OffsetCoordinates(4, 4), Player.Player2);

//             var playerState = env.GetPlayerState(Player.Player1);
//             playerState.Amount = 15;
            
//             var mask = env.GetAvailableBuildingCoordinatesMask(Unit.Tower);
//             Assert.That(mask, Has.Length.EqualTo(env.CellCount));

//             var availableCoordinatesCount = mask.Sum();
//             Assert.That(availableCoordinatesCount, Is.EqualTo(2));
            
//             Assert.That(mask[1], Is.EqualTo(1));
//             Assert.That(mask[5], Is.EqualTo(1));
//         }
        
//         [Test]
//         public void GetAvailableBuildingCoordinatesMask_Opponent()
//         {
//             var env = new Environment(width: 5, height: 5);
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
//             env.CreateBase(new OffsetCoordinates(4, 4), Player.Player2);

//             env.SetPlayer(Player.Player2);
            
//             var playerState = env.GetPlayerState(Player.Player2);
//             playerState.Amount = 15;
            
//             var mask = env.GetAvailableBuildingCoordinatesMask(Unit.Tower);
//             Assert.That(mask, Has.Length.EqualTo(env.CellCount));

//             var availableCoordinatesCount = mask.Sum();
//             Assert.That(availableCoordinatesCount, Is.EqualTo(2));
            
//             Assert.That(mask[17], Is.EqualTo(1));
//             Assert.That(mask[21], Is.EqualTo(1));
//         }

//         [Test]
//         [TestCase(0, 0, 0, 0, 0)]
//         [TestCase(10, 1, 0, 0, 0)]
//         [TestCase(20, 1, 1, 0, 0)]
//         [TestCase(30, 1, 1, 1, 0)]
//         [TestCase(40, 1, 1, 1, 1)]
//         public void GetAvailableEntityMask(int amount, int m0, int m1, int m2, int m3)
//         {
//             var env = new Environment(width: 5, height: 5);
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);

//             var playerState = env.GetPlayerState(Player.Player1);
//             playerState.Amount = amount;
            
//             var mask = env.GetAvailableEntityMask();
//             Assert.That(mask, Is.EqualTo(new[] { m0, m1, m2, m3 }));
//         }

//         [Test]
//         public void GetAvailableEntityCoordinatesMask_EmptyCondition()
//         {
//             var env = new Environment(width: 5, height: 5);
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);

//             var playerState = env.GetPlayerState(Player.Player1);
//             playerState.Amount = 10;
            
//             var mask = env.GetAvailableEntityCoordinatesMask(Unit.Peasant);
//             Assert.That(mask, Has.Length.EqualTo(env.CellCount));

//             var availableCoordinatesCount = mask.Sum();
//             Assert.That(availableCoordinatesCount, Is.EqualTo(6));

//             Assert.That(mask[1], Is.EqualTo(1));
//             Assert.That(mask[2], Is.EqualTo(1));
//             Assert.That(mask[5], Is.EqualTo(1));
//             Assert.That(mask[6], Is.EqualTo(1));
//             Assert.That(mask[9], Is.EqualTo(1));
//             Assert.That(mask[10], Is.EqualTo(1));
//         }
        
//         [Test]
//         public void GetAvailableEntityCoordinatesMask_MergeCondition_CanMerge()
//         {
//             var env = new Environment(width: 5, height: 5);
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);

//             var playerState = env.GetPlayerState(Player.Player1);
//             playerState.Amount = 60;

//             env.Place(new OffsetCoordinates(1, 0), Unit.Peasant);
            
//             var mask = env.GetAvailableEntityCoordinatesMask(Unit.Peasant);
//             Assert.That(mask, Has.Length.EqualTo(env.CellCount));

//             var availableCoordinatesCount = mask.Sum();
//             Assert.That(availableCoordinatesCount, Is.EqualTo(6));

//             Assert.That(mask[1], Is.EqualTo(1));
//             Assert.That(mask[2], Is.EqualTo(1));
//             Assert.That(mask[5], Is.EqualTo(1));
//             Assert.That(mask[6], Is.EqualTo(1));
//             Assert.That(mask[9], Is.EqualTo(1));
//             Assert.That(mask[10], Is.EqualTo(1));
//         }
        
//         [Test]
//         public void GetAvailableEntityCoordinatesMask_MergeCondition_CannotMerge()
//         {
//             var env = new Environment(width: 5, height: 5);
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);

//             var playerState = env.GetPlayerState(Player.Player1);
//             playerState.Amount = 60;

//             env.Place(new OffsetCoordinates(1, 0), Unit.Knight);
            
//             var mask = env.GetAvailableEntityCoordinatesMask(Unit.Knight);
//             Assert.That(mask, Has.Length.EqualTo(env.CellCount));

//             var availableCoordinatesCount = mask.Sum();
//             Assert.That(availableCoordinatesCount, Is.EqualTo(5));

//             Assert.That(mask[2], Is.EqualTo(1));
//             Assert.That(mask[5], Is.EqualTo(1));
//             Assert.That(mask[6], Is.EqualTo(1));
//             Assert.That(mask[9], Is.EqualTo(1));
//             Assert.That(mask[10], Is.EqualTo(1));
//         }
        
//         [Test]
//         public void GetAvailableEntityCoordinatesMask_AttackCondition_CanAttack()
//         {
//             var env = new Environment(width: 5, height: 5);
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
//             env.CreateBase(new OffsetCoordinates(3, 0), Player.Player2);
        
//             var playerState = env.GetPlayerState(Player.Player1);
//             playerState.Amount = 10;
            
//             var mask = env.GetAvailableEntityCoordinatesMask(Unit.Spearman);
//             Assert.That(mask, Has.Length.EqualTo(env.CellCount));
        
//             var availableCoordinatesCount = mask.Sum();
//             Assert.That(availableCoordinatesCount, Is.EqualTo(6));
        
//             Assert.That(mask[1], Is.EqualTo(1));
//             Assert.That(mask[2], Is.EqualTo(1));
//             Assert.That(mask[5], Is.EqualTo(1));
//             Assert.That(mask[6], Is.EqualTo(1));
//             Assert.That(mask[9], Is.EqualTo(1));
//             Assert.That(mask[10], Is.EqualTo(1));
//         }
        
//         [Test]
//         public void GetAvailableEntityCoordinatesMask_AttackCondition_CannotAttack()
//         {
//             var env = new Environment(width: 5, height: 5);
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);
//             env.CreateBase(new OffsetCoordinates(3, 0), Player.Player2);
        
//             var playerState = env.GetPlayerState(Player.Player1);
//             playerState.Amount = 10;
            
//             var mask = env.GetAvailableEntityCoordinatesMask(Unit.Peasant);
//             Assert.That(mask, Has.Length.EqualTo(env.CellCount));
        
//             var availableCoordinatesCount = mask.Sum();
//             Assert.That(availableCoordinatesCount, Is.EqualTo(5));
        
//             Assert.That(mask[1], Is.EqualTo(1));
//             Assert.That(mask[5], Is.EqualTo(1));
//             Assert.That(mask[6], Is.EqualTo(1));
//             Assert.That(mask[9], Is.EqualTo(1));
//             Assert.That(mask[10], Is.EqualTo(1));
//         }
        
//         [Test]
//         public void GetMoveFromCoordinatesMask()
//         {
//             var env = new Environment(width: 5, height: 5);
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);

//             var playerState = env.GetPlayerState(Player.Player1);
//             playerState.Amount = 10;
            
//             env.Place(new OffsetCoordinates(1, 0), Unit.Peasant);

//             var mask = env.GetMoveFromCoordinatesMask();
//             Assert.That(mask, Has.Length.EqualTo(env.CellCount));

//             var availableCoordinatesCount = mask.Sum();
//             Assert.That(availableCoordinatesCount, Is.EqualTo(1));

//             Assert.That(mask[1], Is.EqualTo(1));
//         }
        
//         [Test]
//         public void GetMoveToCoordinatesMask()
//         {
//             var env = new Environment(width: 5, height: 5);
//             env.CreateBase(new OffsetCoordinates(0, 0), Player.Player1);

//             var playerState = env.GetPlayerState(Player.Player1);
//             playerState.Amount = 10;
            
//             env.Place(new OffsetCoordinates(1, 0), Unit.Peasant);

//             var mask = env.GetMoveToCoordinatesMask(new OffsetCoordinates(1, 0));
//             Assert.That(mask, Has.Length.EqualTo(env.CellCount));

//             var availableCoordinatesCount = mask.Sum();
//             Assert.That(availableCoordinatesCount, Is.EqualTo(5));

//             Assert.That(mask[2], Is.EqualTo(1));
//             Assert.That(mask[5], Is.EqualTo(1));
//             Assert.That(mask[6], Is.EqualTo(1));
//             Assert.That(mask[9], Is.EqualTo(1));
//             Assert.That(mask[10], Is.EqualTo(1));
//         }

//         #endregion
        
//         #region RL
        
//         [Test]
//         public void ResetEnvironment()
//         {
//             var config = new EnvironmentConfiguration(width: 5, height: 5);
//             Environment.InitializeConfiguration(config);
//             var state = Environment.Reset();
            
//             Assert.That(Environment.GetUnit(state, 0), Is.EqualTo(Unit.Fort));
//             Assert.That(Environment.GetUnit(state, 1), Is.EqualTo(Unit.None));
//             Assert.That(Environment.GetUnit(state, 5), Is.EqualTo(Unit.None));
//             Assert.That(Environment.GetPlayer(state, 0), Is.EqualTo(Player.Player1));
//             Assert.That(Environment.GetPlayer(state, 1), Is.EqualTo(Player.Player1));
//             Assert.That(Environment.GetPlayer(state, 5), Is.EqualTo(Player.Player1));
            
//             Assert.That(Environment.GetUnit(state, 22), Is.EqualTo(Unit.Fort));
//             Assert.That(Environment.GetUnit(state, 17), Is.EqualTo(Unit.None));
//             Assert.That(Environment.GetUnit(state, 21), Is.EqualTo(Unit.None));
//             Assert.That(Environment.GetPlayer(state, 22), Is.EqualTo(Player.Player2));
//             Assert.That(Environment.GetPlayer(state, 17), Is.EqualTo(Player.Player2));
//             Assert.That(Environment.GetPlayer(state, 21), Is.EqualTo(Player.Player2));
            
//             Assert.That(Environment.GetUnit(state, 11), Is.EqualTo(Unit.None));
//             Assert.That(Environment.GetPlayer(state, 11), Is.EqualTo(Player.None));
//         }

//         [Test]
//         public void StepEnvironment()
//         {
//             var config = new EnvironmentConfiguration(width: 5, height: 5);
//             Environment.InitializeConfiguration(config);
//             var state = Environment.Reset();
//             var action = new EnvironmentAction
//             {
//                 Type = ActionType.BuildTower,
//                 BuildTowerP0 = 1
//             };

//             var (nextState, _) = Environment.Step(state, action);
            
//             Assert.That(Environment.GetUnit(state, 1), Is.EqualTo(Unit.None));
//             Assert.That(Environment.GetUnit(nextState, 1), Is.EqualTo(Unit.Tower));
//         }
        
//         [Test]
//         public void ValidateBuildTower()
//         {
//             var config = new EnvironmentConfiguration(width: 5, height: 5);
//             Environment.InitializeConfiguration(config);
//             var state = Environment.Reset();
//             var mask = Environment.GetBuildTowerMaskCellIndex(state);
            
//             Assert.That(mask, Has.Length.EqualTo(23));

//             var availableCoordinatesCount = mask.Sum();
//             Assert.That(availableCoordinatesCount, Is.EqualTo(2));
            
//             Assert.That(mask[1], Is.EqualTo(1));
//             Assert.That(mask[5], Is.EqualTo(1));
//         }

//         // [Test]
//         // public void ValidateBuildTower()
//         // {
//         //     var config = new EnvironmentConfiguration(width: 5, height: 5);
//         //     var state = Environment.Reset(config);
//         //
//         //     {
//         //         var action = new EnvironmentAction
//         //         {
//         //             Type = ActionType.BuildTower,
//         //             BuildTowerCellIndex = 1
//         //         };
//         //         
//         //         var validated = Environment.Validate(state, action);
//         //
//         //         Assert.That(validated, Is.True);
//         //     }
//         //
//         //     {
//         //         var action = new EnvironmentAction
//         //         {
//         //             Type = ActionType.BuildTower,
//         //             BuildTowerCellIndex = 1
//         //         };
//         //         
//         //         var nextState = Environment.Step(state, action);
//         //         var validated = Environment.Validate(nextState, action);
//         //
//         //         Assert.That(validated, Is.False);
//         //     }
//         //     
//         //     {
//         //         var action = new EnvironmentAction
//         //         {
//         //             Type = ActionType.BuildTower,
//         //             BuildTowerCellIndex = 2
//         //         };
//         //         
//         //         var validated = Environment.Validate(state, action);
//         //
//         //         Assert.That(validated, Is.False);
//         //     }
//         // }
//         //
//         // [Test]
//         // public void ValidatePlaceUnit()
//         // {
//         //     // Place on player territory -> check empty cell or can merge.
//         //     // Place on neutral territory -> just place.
//         //     // Place on enemy territory -> attack point of unit is high that defence point.
//         // }
//         //
//         // [Test]
//         // public void ValidateMoveUnit()
//         // {
//         //     // Check if select cell has unit and belongs to player.
//         //     // Place on player territory -> check empty cell or can merge.
//         //     // Place on neutral territory -> just place.
//         //     // Place on enemy territory -> attack point of unit is high that defence point.
//         // }
//         //
//         // [Test]
//         // public void ValidateEndTurn()
//         // {
//         //     
//         // }

//         #endregion
//     }
// }
