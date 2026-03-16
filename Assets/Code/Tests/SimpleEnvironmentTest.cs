// using System;
// using System.Collections.Generic;
// using NUnit.Framework;
// using UniStepRL;
// using UnityEditor.VersionControl;

// namespace Tests
// {
//     public class SimpleEnvironmentTest
//     {
//         private EnvironmentState m_State;

//         [SetUp]
//         public void SetUp()
//         {
//             m_State = new EnvironmentState
//             {
//                 Units = new Unit[5],
//                 Players = new Player[5],
//                 EntitiesLookUpTable = new int[5],
//                 Entities = new List<Entity>(5)
//             };

//             Array.Fill(m_State.EntitiesLookUpTable, -1);
//         }
        
//         [Test]
//         public void RemoveEntityWithSwap_SingleEntity()
//         {
//             PlaceEntity(m_State, Unit.Baron, 4);
            
//             Assert.That(m_State.Entities, Has.Count.EqualTo(1));
//             Assert.That(m_State.EntitiesLookUpTable[4], Is.EqualTo(0));
            
//             SimpleEnvironment.RemoveEntityWithSwap(m_State, 0);
            
//             Assert.That(m_State.Entities, Has.Count.EqualTo(0));
//             Assert.That(m_State.EntitiesLookUpTable, Is.All.EqualTo(-1));
//         }
        
//         [Test]
//         public void RemoveEntityWithSwap_TwoEntities_RemoveFirstEntity()
//         {
//             PlaceEntity(m_State, Unit.Baron, 4);
//             PlaceEntity(m_State, Unit.Baron, 2);
            
//             Assert.That(m_State.Entities, Has.Count.EqualTo(2));
//             Assert.That(m_State.EntitiesLookUpTable[4], Is.EqualTo(0));
//             Assert.That(m_State.EntitiesLookUpTable[2], Is.EqualTo(1));
            
//             SimpleEnvironment.RemoveEntityWithSwap(m_State, 0);
            
//             Assert.That(m_State.Entities, Has.Count.EqualTo(1));
//             Assert.That(m_State.EntitiesLookUpTable[0], Is.EqualTo(-1));
//             Assert.That(m_State.EntitiesLookUpTable[1], Is.EqualTo(-1));
//             Assert.That(m_State.EntitiesLookUpTable[2], Is.EqualTo(0));
//             Assert.That(m_State.EntitiesLookUpTable[3], Is.EqualTo(-1));
//             Assert.That(m_State.EntitiesLookUpTable[4], Is.EqualTo(-1));
//         }
        
//         [Test]
//         public void RemoveEntityWithSwap_TwoEntities_RemoveSecondEntity()
//         {
//             PlaceEntity(m_State, Unit.Baron, 4);
//             PlaceEntity(m_State, Unit.Baron, 2);
            
//             Assert.That(m_State.Entities, Has.Count.EqualTo(2));
//             Assert.That(m_State.EntitiesLookUpTable[4], Is.EqualTo(0));
//             Assert.That(m_State.EntitiesLookUpTable[2], Is.EqualTo(1));
            
//             SimpleEnvironment.RemoveEntityWithSwap(m_State, 1);
            
//             Assert.That(m_State.Entities, Has.Count.EqualTo(1));
//             Assert.That(m_State.EntitiesLookUpTable[0], Is.EqualTo(-1));
//             Assert.That(m_State.EntitiesLookUpTable[1], Is.EqualTo(-1));
//             Assert.That(m_State.EntitiesLookUpTable[2], Is.EqualTo(-1));
//             Assert.That(m_State.EntitiesLookUpTable[3], Is.EqualTo(-1));
//             Assert.That(m_State.EntitiesLookUpTable[4], Is.EqualTo(0));
//         }
        
//         [Test]
//         public void RemoveEntityWithSwap_ThreeEntities_RemoveFirstEntity()
//         {
//             PlaceEntity(m_State, Unit.Baron, 4);
//             PlaceEntity(m_State, Unit.Baron, 2);
//             PlaceEntity(m_State, Unit.Baron, 0);
            
//             Assert.That(m_State.Entities, Has.Count.EqualTo(3));
//             Assert.That(m_State.EntitiesLookUpTable[4], Is.EqualTo(0));
//             Assert.That(m_State.EntitiesLookUpTable[2], Is.EqualTo(1));
//             Assert.That(m_State.EntitiesLookUpTable[0], Is.EqualTo(2));
            
//             SimpleEnvironment.RemoveEntityWithSwap(m_State, 0);
            
//             Assert.That(m_State.Entities, Has.Count.EqualTo(2));
//             Assert.That(m_State.EntitiesLookUpTable[0], Is.EqualTo(0));
//             Assert.That(m_State.EntitiesLookUpTable[1], Is.EqualTo(-1));
//             Assert.That(m_State.EntitiesLookUpTable[2], Is.EqualTo(1));
//             Assert.That(m_State.EntitiesLookUpTable[3], Is.EqualTo(-1));
//             Assert.That(m_State.EntitiesLookUpTable[4], Is.EqualTo(-1));
//         }
        
//         [Test]
//         public void RemoveEntityWithSwap_ThreeEntities_RemoveSecondEntity()
//         {
//             PlaceEntity(m_State, Unit.Baron, 4);
//             PlaceEntity(m_State, Unit.Baron, 2);
//             PlaceEntity(m_State, Unit.Baron, 0);
            
//             Assert.That(m_State.Entities, Has.Count.EqualTo(3));
//             Assert.That(m_State.EntitiesLookUpTable[4], Is.EqualTo(0));
//             Assert.That(m_State.EntitiesLookUpTable[2], Is.EqualTo(1));
//             Assert.That(m_State.EntitiesLookUpTable[0], Is.EqualTo(2));
            
//             SimpleEnvironment.RemoveEntityWithSwap(m_State, 1);
            
//             Assert.That(m_State.Entities, Has.Count.EqualTo(2));
//             Assert.That(m_State.EntitiesLookUpTable[0], Is.EqualTo(1));
//             Assert.That(m_State.EntitiesLookUpTable[1], Is.EqualTo(-1));
//             Assert.That(m_State.EntitiesLookUpTable[2], Is.EqualTo(-1));
//             Assert.That(m_State.EntitiesLookUpTable[3], Is.EqualTo(-1));
//             Assert.That(m_State.EntitiesLookUpTable[4], Is.EqualTo(0));
//         }
        
//         [Test]
//         public void RemoveEntityWithSwap_ThreeEntities_RemoveThirdEntity()
//         {
//             PlaceEntity(m_State, Unit.Baron, 4);
//             PlaceEntity(m_State, Unit.Baron, 2);
//             PlaceEntity(m_State, Unit.Baron, 0);
            
//             Assert.That(m_State.Entities, Has.Count.EqualTo(3));
//             Assert.That(m_State.EntitiesLookUpTable[0], Is.EqualTo(2));
//             Assert.That(m_State.EntitiesLookUpTable[1], Is.EqualTo(-1));
//             Assert.That(m_State.EntitiesLookUpTable[2], Is.EqualTo(1));
//             Assert.That(m_State.EntitiesLookUpTable[3], Is.EqualTo(-1));
//             Assert.That(m_State.EntitiesLookUpTable[4], Is.EqualTo(0));
            
//             SimpleEnvironment.RemoveEntityWithSwap(m_State, 2);
            
//             Assert.That(m_State.Entities, Has.Count.EqualTo(2));
//             Assert.That(m_State.EntitiesLookUpTable[0], Is.EqualTo(-1));
//             Assert.That(m_State.EntitiesLookUpTable[1], Is.EqualTo(-1));
//             Assert.That(m_State.EntitiesLookUpTable[2], Is.EqualTo(1));
//             Assert.That(m_State.EntitiesLookUpTable[3], Is.EqualTo(-1));
//             Assert.That(m_State.EntitiesLookUpTable[4], Is.EqualTo(0));
//         }

//         private static void PlaceEntity(EnvironmentState state, Unit unit, int index, Player player = Player.Player1)
//         {
//             state.Units[index] = Unit.Baron;
//             state.Players[index] = player;
//             state.EntitiesLookUpTable[index] = state.Entities.Count;
//             state.Entities.Add(new Entity
//             {
//                 Coordinates = index,
//                 Player = player,
//                 Unit = unit,
//                 Moved = false
//             });
//         }
//     }
// }
