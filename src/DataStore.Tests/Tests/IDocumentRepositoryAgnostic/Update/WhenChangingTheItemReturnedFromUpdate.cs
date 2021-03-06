namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    using System;
    using System.Linq;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenChangingTheItemReturnedFromUpdate
    {
        private readonly Guid carId;

        private readonly ITestHarness testHarness;

        public WhenChangingTheItemReturnedFromUpdate()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenChangingTheItemReturnedFromUpdate));

            this.carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = this.carId,
                Make = "Volvo"
            };
            this.testHarness.AddToDatabase(existingCar);

            //read from db to pickup changes to properties made by datastore oncreate
            var existingCarFromDb = this.testHarness.DataStore.ReadActiveById<Car>(this.carId).Result;
            existingCarFromDb.Make = "Ford";

            var result = this.testHarness.DataStore.Update(existingCarFromDb).Result;

            //change the id before committing, if not cloned this would cause the item not to be found
            result.id = Guid.NewGuid();

            //When
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldNotAffectTheUpdateWhenCommittedBecauseItIsCloned()
        {
            Assert.Equal("Ford", this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Make);
            Assert.Equal("Ford", this.testHarness.DataStore.ReadActiveById<Car>(this.carId).Result.Make);
        }
    }
}