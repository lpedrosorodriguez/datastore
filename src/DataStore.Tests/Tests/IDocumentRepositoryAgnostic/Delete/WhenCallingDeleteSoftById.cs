namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingDeleteSoftById
    {
        public WhenCallingDeleteSoftById()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingDeleteSoftById));

            carId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            //When
            testHarness.DataStore.DeleteSoftById<Car>(carId).Wait();
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly Guid carId;

        [Fact]
        public async void ItShouldPersistChangesToTheDatabase()
        {
            Assert.NotNull(testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is SoftDeleteOperation<Car>));
            Assert.Null(testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedSoftDeleteOperation<Car>));
            Assert.False(testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Single().Active);
            Assert.Empty(await testHarness.DataStore.ReadActive<Car>());
            Assert.NotEmpty(await testHarness.DataStore.Read<Car>());
        }
    }
}