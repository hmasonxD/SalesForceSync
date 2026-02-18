using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SalesForceSync.Controllers;
using SalesForceSync.Data;
using SalesForceSync.Models;
using SalesForceSync.Services;

namespace SalesForceSync.Tests
{
    [TestClass]
    public class SyncControllerTests
    {
        private AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [TestMethod]
        public async Task GetContacts_ReturnsAllContacts()
        {
            var dbContext = CreateInMemoryDbContext();
            dbContext.Contacts.AddRange(
                new Contact { FirstName = "John", LastName = "Doe", Email = "john@test.com" },
                new Contact { FirstName = "Jane", LastName = "Doe", Email = "jane@test.com" }
            );
            await dbContext.SaveChangesAsync();

            var mockContactService = new Mock<SalesforceContactService>(null, null, null);
            var mockLogger = new Mock<ILogger<SyncController>>();
            var controller = new SyncController(mockContactService.Object, dbContext, mockLogger.Object);

            var result = await controller.GetContacts();
            Assert.IsInstanceOfType<OkObjectResult>(result);
        }

        [TestMethod]
        public async Task GetContact_WithValidId_ReturnsContact()
        {
            var dbContext = CreateInMemoryDbContext();
            var contact = new Contact { FirstName = "John", LastName = "Doe", Email = "john@test.com" };
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();

            var mockContactService = new Mock<SalesforceContactService>(null, null, null);
            var mockLogger = new Mock<ILogger<SyncController>>();
            var controller = new SyncController(mockContactService.Object, dbContext, mockLogger.Object);

            var result = await controller.GetContact(contact.Id);
            Assert.IsInstanceOfType<OkObjectResult>(result);
        }

        [TestMethod]
        public async Task GetContact_WithInvalidId_Returns404()
        {
            var dbContext = CreateInMemoryDbContext();
            var mockContactService = new Mock<SalesforceContactService>(null, null, null);
            var mockLogger = new Mock<ILogger<SyncController>>();
            var controller = new SyncController(mockContactService.Object, dbContext, mockLogger.Object);

            var result = await controller.GetContact(999);
            Assert.IsInstanceOfType<NotFoundObjectResult>(result);
        }

        [TestMethod]
        public async Task DeleteContact_WithValidId_DeletesContact()
        {
            var dbContext = CreateInMemoryDbContext();
            var contact = new Contact { FirstName = "John", LastName = "Doe", Email = "john@test.com" };
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();

            var mockContactService = new Mock<SalesforceContactService>(null, null, null);
            var mockLogger = new Mock<ILogger<SyncController>>();
            var controller = new SyncController(mockContactService.Object, dbContext, mockLogger.Object);

            var result = await controller.DeleteContact(contact.Id);
            Assert.IsInstanceOfType<OkObjectResult>(result);
            Assert.IsNull(await dbContext.Contacts.FindAsync(contact.Id));
        }

        [TestMethod]
        public async Task DeleteContact_WithInvalidId_Returns404()
        {
            var dbContext = CreateInMemoryDbContext();
            var mockContactService = new Mock<SalesforceContactService>(null, null, null);
            var mockLogger = new Mock<ILogger<SyncController>>();
            var controller = new SyncController(mockContactService.Object, dbContext, mockLogger.Object);

            var result = await controller.DeleteContact(999);
            Assert.IsInstanceOfType<NotFoundObjectResult>(result);
        }

        [TestMethod]
        public async Task GetContacts_WithSearch_ReturnsFilteredResults()
        {
            var dbContext = CreateInMemoryDbContext();
            dbContext.Contacts.AddRange(
                new Contact { FirstName = "John", LastName = "Doe", Email = "john@test.com" },
                new Contact { FirstName = "Jane", LastName = "Doe", Email = "jane@test.com" },
                new Contact { FirstName = "Bob", LastName = "Burgers", Email = "bob@test.com" }
            );
            await dbContext.SaveChangesAsync();

            var mockContactService = new Mock<SalesforceContactService>(null, null, null);
            var mockLogger = new Mock<ILogger<SyncController>>();
            var controller = new SyncController(mockContactService.Object, dbContext, mockLogger.Object);

            var result = await controller.GetContacts(search: "john");
            Assert.IsInstanceOfType<OkObjectResult>(result);
        }
    }
}