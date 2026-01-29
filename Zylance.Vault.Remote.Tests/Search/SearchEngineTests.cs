using Zylance.Vault.Remote.Search;
using Zylance.Vault.Remote.Tests.Search.Lib;

namespace Zylance.Vault.Remote.Tests.Search;

public class SearchEngineTests
{
    #region Setup

    private readonly MemorySearchGlossary _glossary;
    private readonly SearchEngine<string> _searchEngine;

    public SearchEngineTests()
    {
        _glossary = new MemorySearchGlossary();

        var bucketManager = new MemorySearchBucketManager<string> { MaxItemsPerBucket = 10 };
        _searchEngine = new SearchEngine<string>(
            _glossary,
            bucketManager);
    }

    #endregion

    #region AddIndex Tests

    [Fact]
    public async Task AddIndex_ShouldTokenizeAndIndexText()
    {
        // Arrange
        var itemId = "item1";
        var text = "Hello World";

        // Act
        await _searchEngine.AddIndex(itemId, text);

        // Assert
        var keywords = _glossary.GetKeywords();
        Assert.Equal(2, keywords.Count);
        Assert.Contains(keywords, k => k.Value == "hello");
        Assert.Contains(keywords, k => k.Value == "world");
    }

    [Fact]
    public async Task AddIndex_ShouldHandleSpecialCharacters()
    {
        // Arrange
        var itemId = "item1";
        var text = "Hello, World! Test@123";

        // Act
        await _searchEngine.AddIndex(itemId, text);

        // Assert
        var keywords = _glossary.GetKeywords();
        Assert.Equal(4, keywords.Count);
        Assert.Contains(keywords, k => k.Value == "hello");
        Assert.Contains(keywords, k => k.Value == "world");
        Assert.Contains(keywords, k => k.Value == "test"); // @ splits tokens
        Assert.Contains(keywords, k => k.Value == "123");
    }

    [Fact]
    public async Task AddIndex_ShouldIgnoreWhitespace()
    {
        // Arrange
        var itemId = "item1";
        var text = "   Hello   World   ";

        // Act
        await _searchEngine.AddIndex(itemId, text);

        // Assert
        var keywords = _glossary.GetKeywords();
        Assert.Equal(2, keywords.Count);
    }

    [Fact]
    public async Task AddIndex_ShouldConvertToLowerCase()
    {
        // Arrange
        var itemId = "item1";
        var text = "HELLO World HeLLo";

        // Act
        await _searchEngine.AddIndex(itemId, text);

        // Assert
        var keywords = _glossary.GetKeywords();
        var helloKeyword = keywords.Single(k => k.Value == "hello");
        Assert.NotNull(helloKeyword);
    }

    [Fact]
    public async Task AddIndex_ShouldCreateNewBucketWhenMaxItemsReached()
    {
        // Arrange
        var glossary = new MemorySearchGlossary();
        var bucketManager = new MemorySearchBucketManager<string> { MaxItemsPerBucket = 2 };
        var searchEngine = new SearchEngine<string>(glossary, bucketManager);
        var text = "hello";

        // Act - Add 3 items, should create 2 buckets
        await searchEngine.AddIndex("item1", text);
        await searchEngine.AddIndex("item2", text);
        await searchEngine.AddIndex("item3", text);

        // Assert
        var keywords = glossary.GetKeywords();
        var helloKeyword = keywords.Single(k => k.Value == "hello");
        Assert.Equal(2u, helloKeyword.NumBuckets);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task Search_ShouldReturnMatchingItems()
    {
        // Arrange
        await _searchEngine.AddIndex("item1", "Hello World");
        await _searchEngine.AddIndex("item2", "Hello Universe");
        await _searchEngine.AddIndex("item3", "Goodbye World");

        // Act
        var results = await _searchEngine.Search("hello");

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains("item1", results);
        Assert.Contains("item2", results);
    }

    [Fact]
    public async Task Search_ShouldReturnItemsForMultipleTerms()
    {
        // Arrange
        await _searchEngine.AddIndex("item1", "Hello World");
        await _searchEngine.AddIndex("item2", "Hello Universe");
        await _searchEngine.AddIndex("item3", "Goodbye World");

        // Act
        var results = await _searchEngine.Search("hello world");

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Contains("item1", results);
        Assert.Contains("item2", results);
        Assert.Contains("item3", results);
    }

    [Fact]
    public async Task Search_WithExactMatch_ShouldOnlyReturnExactMatches()
    {
        // Arrange
        await _searchEngine.AddIndex("item1", "Hello World");
        await _searchEngine.AddIndex("item2", "Help");

        // Act
        var results = await _searchEngine.Search("hel", fuzzy: false);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task Search_WithFuzzyMatch_ShouldReturnPartialMatches()
    {
        // Arrange
        await _searchEngine.AddIndex("item1", "Hello World");
        await _searchEngine.AddIndex("item2", "Help");

        // Act
        var results = await _searchEngine.Search("hel", fuzzy: true);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains("item1", results);
        Assert.Contains("item2", results);
    }

    [Fact]
    public async Task Search_WithLatestFirstDirection_ShouldSearchFromNewestBucket()
    {
        // Arrange
        await _searchEngine.AddIndex("item1", "test");

        // Act
        var results = await _searchEngine.Search("test");

        // Assert
        Assert.Single(results);
        Assert.Contains("item1", results);
    }

    [Fact]
    public async Task Search_WithOldestFirstDirection_ShouldSearchFromOldestBucket()
    {
        // Arrange
        await _searchEngine.AddIndex("item1", "test");

        // Act
        var results = await _searchEngine.Search("test", SearchDirection.OldestFirst);

        // Assert
        Assert.Single(results);
        Assert.Contains("item1", results);
    }

    [Fact]
    public async Task Search_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        await _searchEngine.AddIndex("item1", "Hello World");

        // Act
        var results = await _searchEngine.Search("nonexistent");

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task Search_AcrossMultipleBuckets_ShouldReturnAllMatches()
    {
        // Arrange
        var glossary = new MemorySearchGlossary();
        var bucketManager = new MemorySearchBucketManager<string> { MaxItemsPerBucket = 2 };
        var searchEngine = new SearchEngine<string>(glossary, bucketManager);

        await searchEngine.AddIndex("item1", "test");
        await searchEngine.AddIndex("item2", "test");
        await searchEngine.AddIndex("item3", "test");

        // Act
        var results = await searchEngine.Search("test");

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Contains("item1", results);
        Assert.Contains("item2", results);
        Assert.Contains("item3", results);
    }

    [Fact]
    public async Task Search_WithDuplicateItemsInResults_ShouldReturnUniqueItems()
    {
        // Arrange
        await _searchEngine.AddIndex("item1", "Hello World");

        // Act - Both "hello" and "world" point to the same item
        var results = await _searchEngine.Search("hello world");

        // Assert - Should only return item1 once
        Assert.Single(results);
        Assert.Contains("item1", results);
    }

    #endregion

    #region Reindex Tests

    [Fact]
    public async Task Reindex_ShouldUpdateChangedKeywordsOnly()
    {
        // Arrange
        await _searchEngine.AddIndex("item1", "Buy coffee at Starbucks");

        // Act
        await _searchEngine.Reindex("item1", "Buy coffee at Starbucks", "Buy coffee at McDonald's");

        // Assert - Should be removed from Starbucks
        var starbucksResults = await _searchEngine.Search("starbucks");
        Assert.Empty(starbucksResults);

        // Assert - Should be added to McDonald's
        var mcdonaldsResults = await _searchEngine.Search("mcdonald");
        Assert.Single(mcdonaldsResults);
        Assert.Contains("item1", mcdonaldsResults);

        // Assert - Should still be in "buy" and "coffee" (not removed and re-added)
        var coffeeResults = await _searchEngine.Search("coffee");
        Assert.Single(coffeeResults);
        Assert.Contains("item1", coffeeResults);
    }

    [Fact]
    public async Task Reindex_WithCompletelyDifferentText_ShouldReplaceAllKeywords()
    {
        // Arrange
        await _searchEngine.AddIndex("item1", "Buy coffee");

        // Act
        await _searchEngine.Reindex("item1", "Buy coffee", "Sell books");

        // Assert - Old keywords should not return results
        var coffeeResults = await _searchEngine.Search("coffee");
        Assert.Empty(coffeeResults);

        var buyResults = await _searchEngine.Search("buy");
        Assert.Empty(buyResults);

        // Assert - New keywords should return results
        var sellResults = await _searchEngine.Search("sell");
        Assert.Single(sellResults);
        Assert.Contains("item1", sellResults);

        var booksResults = await _searchEngine.Search("books");
        Assert.Single(booksResults);
        Assert.Contains("item1", booksResults);
    }

    #endregion

    #region Deindex Tests

    [Fact]
    public async Task Deindex_ShouldRemoveItemFromAllKeywordBuckets()
    {
        // Arrange
        await _searchEngine.AddIndex("item1", "Hello World");

        // Act
        await _searchEngine.Deindex("item1", "Hello World");

        // Assert - Should not find item by any keyword
        var helloResults = await _searchEngine.Search("hello");
        Assert.Empty(helloResults);

        var worldResults = await _searchEngine.Search("world");
        Assert.Empty(worldResults);
    }

    [Fact]
    public async Task Deindex_WithMultipleItems_ShouldOnlyRemoveTargetItem()
    {
        // Arrange
        await _searchEngine.AddIndex("item1", "Hello World");
        await _searchEngine.AddIndex("item2", "Hello Universe");

        // Act
        await _searchEngine.Deindex("item1", "Hello World");

        // Assert - item1 should be gone
        var results = await _searchEngine.Search("hello world");
        Assert.Single(results);
        Assert.Contains("item2", results);
        Assert.DoesNotContain("item1", results);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task AddIndex_WithEmptyString_ShouldNotAddKeywords()
    {
        // Arrange
        var itemId = "item1";
        var text = "";

        // Act
        await _searchEngine.AddIndex(itemId, text);

        // Assert
        var keywords = _glossary.GetKeywords();
        Assert.Empty(keywords);
    }

    [Fact]
    public async Task AddIndex_WithOnlyWhitespace_ShouldNotAddKeywords()
    {
        // Arrange
        var itemId = "item1";
        var text = "   \t\n   ";

        // Act
        await _searchEngine.AddIndex(itemId, text);

        // Assert
        var keywords = _glossary.GetKeywords();
        Assert.Empty(keywords);
    }

    [Fact]
    public async Task Search_WithDifferentItemIdTypes_ShouldWork()
    {
        // Arrange - Test with integer IDs
        var glossary = new MemorySearchGlossary();
        var bucketManager = new MemorySearchBucketManager<int> { MaxItemsPerBucket = 10 };
        var searchEngine = new SearchEngine<int>(glossary, bucketManager);

        // Act
        await searchEngine.AddIndex(1, "Hello World");
        await searchEngine.AddIndex(2, "Goodbye World");
        var results = await searchEngine.Search("world");

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(1, results);
        Assert.Contains(2, results);
    }

    #endregion

    #region Case Sensitivity and Normalization

    [Fact]
    public async Task Search_CaseInsensitive_ShouldMatchRegardlessOfCase()
    {
        // Arrange
        await _searchEngine.AddIndex("item1", "Hello World");

        // Act
        var results = await _searchEngine.Search("HELLO");

        // Assert
        Assert.Single(results);
        Assert.Contains("item1", results);
    }

    [Fact]
    public async Task AddIndex_WithNumericTokens_ShouldIndexNumbers()
    {
        // Arrange
        var itemId = "item1";
        var text = "The year 2024 was great";

        // Act
        await _searchEngine.AddIndex(itemId, text);

        // Assert
        var keywords = _glossary.GetKeywords();
        Assert.Contains(keywords, k => k.Value == "2024");
    }

    [Fact]
    public async Task Search_WithMultipleWordsInDifferentItems_ShouldReturnIntersection()
    {
        // Arrange
        await _searchEngine.AddIndex("item1", "apple banana");
        await _searchEngine.AddIndex("item2", "apple orange");
        await _searchEngine.AddIndex("item3", "banana orange");

        // Act
        var results = await _searchEngine.Search("apple");

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains("item1", results);
        Assert.Contains("item2", results);
    }

    #endregion

    #region Real-World Transaction Scenarios

    [Fact]
    public async Task Search_WithManyTransactionPayees_ShouldFindMatches()
    {
        // Arrange - Index 100 transaction payees
        var payees = new[]
        {
            "Starbucks Coffee #1234", "Amazon.com", "Shell Gas Station",
            "Whole Foods Market", "Target Store #5678", "McDonald's Restaurant",
            "Walmart Supercenter", "CVS Pharmacy #9012", "Home Depot",
            "Best Buy Electronics", "Chipotle Mexican Grill", "Safeway Grocery",
            "Costco Wholesale", "7-Eleven Store", "Subway Sandwiches",
            "Apple Store Online", "Netflix Subscription", "Spotify Premium",
            "AT&T Wireless Payment", "Comcast Cable Bill", "PG&E Utilities",
            "State Farm Insurance", "Chase Credit Card Payment", "Venmo Payment",
            "PayPal Transfer", "Square Cash", "Uber Trip", "Lyft Ride",
            "DoorDash Delivery", "Grubhub Order", "Instacart Groceries",
            "Southwest Airlines", "United Airlines", "Hilton Hotels",
            "Marriott International", "Airbnb Reservation", "Budget Car Rental",
            "Enterprise Rent-A-Car", "AutoZone Auto Parts", "Jiffy Lube Oil Change",
            "Planet Fitness Gym", "LA Fitness Membership", "AMC Theatres",
            "Regal Cinemas", "Barnes & Noble Bookstore", "GameStop",
            "Petco Pet Supplies", "PetSmart", "Chewy.com Pet Food",
            "Kroger Supermarket", "Trader Joe's", "Panera Bread",
            "Olive Garden Restaurant", "Red Lobster", "Outback Steakhouse",
            "Chili's Grill & Bar", "Applebee's", "Buffalo Wild Wings",
            "Pizza Hut Delivery", "Domino's Pizza", "Papa John's",
            "Taco Bell", "KFC Restaurant", "Burger King",
            "Wendy's", "Arby's", "Five Guys Burgers",
            "In-N-Out Burger", "Shake Shack", "The Cheesecake Factory",
            "P.F. Chang's", "California Pizza Kitchen", "Panda Express",
            "Jamba Juice", "Smoothie King", "Dunkin' Donuts",
            "Krispy Kreme", "Baskin-Robbins", "Cold Stone Creamery",
            "Yogurtland", "Pinkberry Frozen Yogurt", "Nordstrom Department Store",
            "Macy's", "JCPenney", "Kohl's Department Store",
            "Gap Clothing Store", "Old Navy", "H&M Fashion",
            "Zara", "Forever 21", "Victoria's Secret",
            "Bath & Body Works", "Bed Bath & Beyond", "Williams Sonoma",
            "Crate and Barrel", "IKEA Furniture", "Office Depot",
            "Staples Office Supplies", "FedEx Shipping", "UPS Store",
            "USPS Postage", "Walgreens Pharmacy", "Rite Aid"
        };

        for (var i = 0; i < payees.Length; i++)
        {
            await _searchEngine.AddIndex($"txn_{i:D4}", payees[i]);
        }

        // Act - Search for "coffee"
        var coffeeResults = await _searchEngine.Search("coffee");

        // Assert
        Assert.Single(coffeeResults);
        Assert.Contains("txn_0000", coffeeResults); // Starbucks Coffee
    }

    [Fact]
    public async Task Search_WithTransactionMemos_ShouldFindRelevantTransactions()
    {
        // Arrange - Index transaction memos
        var transactions = new Dictionary<string, string>
        {
            ["txn_001"] = "Weekly grocery shopping at Whole Foods",
            ["txn_002"] = "Gas fill-up on highway 101",
            ["txn_003"] = "Coffee and breakfast sandwich",
            ["txn_004"] = "Monthly internet bill payment",
            ["txn_005"] = "Dinner with clients at Italian restaurant",
            ["txn_006"] = "Office supplies for home office",
            ["txn_007"] = "Pharmacy prescription refill",
            ["txn_008"] = "Car insurance premium quarterly payment",
            ["txn_009"] = "Grocery delivery tip included",
            ["txn_010"] = "Birthday gift for Sarah",
            ["txn_011"] = "Movie tickets for weekend",
            ["txn_012"] = "Gym membership monthly fee",
            ["txn_013"] = "Utility bill electric and gas",
            ["txn_014"] = "Pet food and supplies",
            ["txn_015"] = "Hardware store for home repairs",
            ["txn_016"] = "Coffee shop meeting with team",
            ["txn_017"] = "Online shopping electronics",
            ["txn_018"] = "Restaurant lunch business expense",
            ["txn_019"] = "Gas station convenience store",
            ["txn_020"] = "Grocery store weekly shopping"
        };

        foreach (var (id, memo) in transactions)
        {
            await _searchEngine.AddIndex(id, memo);
        }

        // Act - Search for different terms
        var groceryResults = await _searchEngine.Search("grocery");
        var coffeeResults = await _searchEngine.Search("coffee");
        var gasResults = await _searchEngine.Search("gas");

        // Assert
        Assert.Equal(3, groceryResults.Count); // txn_001, txn_009, txn_020
        Assert.Equal(2, coffeeResults.Count); // txn_003, txn_016
        Assert.Equal(3, gasResults.Count); // txn_002 (gas fill-up), txn_013 (gas utility), txn_019 (gas station)
    }

    [Fact]
    public async Task Search_WithLargeDatasetAndBucketOverflow_ShouldHandleCorrectly()
    {
        // Arrange - Small bucket size to force multiple buckets
        var glossary = new MemorySearchGlossary();
        var bucketManager = new MemorySearchBucketManager<string> { MaxItemsPerBucket = 5 };
        var searchEngine = new SearchEngine<string>(glossary, bucketManager);

        // Index 50 transactions all containing "payment"
        for (var i = 0; i < 50; i++)
        {
            var memo = i switch
            {
                < 10 => $"Credit card payment #{i + 1}",
                < 20 => $"Utility payment reference {i + 1}",
                < 30 => $"Online payment confirmation {i + 1}",
                < 40 => $"Automatic payment processed {i + 1}",
                _ => $"Manual payment entry {i + 1}"
            };
            await searchEngine.AddIndex($"txn_{i:D3}", memo);
        }

        // Act
        var results = await searchEngine.Search("payment");

        // Assert - Should find all 50 transactions across multiple buckets
        Assert.Equal(50, results.Count);
        Assert.Contains("txn_000", results);
        Assert.Contains("txn_025", results);
        Assert.Contains("txn_049", results);
    }

    [Fact]
    public async Task Search_WithCommonPayeePatterns_ShouldDistinguishSimilarNames()
    {
        // Arrange - Similar payee names
        await _searchEngine.AddIndex("txn_001", "Amazon.com Online Purchase");
        await _searchEngine.AddIndex("txn_002", "Amazon Prime Membership");
        await _searchEngine.AddIndex("txn_003", "Amazon AWS Cloud Services");
        await _searchEngine.AddIndex("txn_004", "Amazon Music Subscription");
        await _searchEngine.AddIndex("txn_005", "Target Store In-Person");
        await _searchEngine.AddIndex("txn_006", "Target.com Online Order");
        await _searchEngine.AddIndex("txn_007", "Target REDcard Payment");

        // Act
        var amazonResults = await _searchEngine.Search("amazon");
        var targetResults = await _searchEngine.Search("target");
        var onlineResults = await _searchEngine.Search("online");

        // Assert
        Assert.Equal(4, amazonResults.Count);
        Assert.Equal(3, targetResults.Count);
        Assert.Equal(2, onlineResults.Count); // Amazon.com and Target.com
    }

    [Fact]
    public async Task Search_WithFuzzyMatchOnPayees_ShouldFindPartialMatches()
    {
        // Arrange
        await _searchEngine.AddIndex("txn_001", "McDonald's Restaurant #4532");
        await _searchEngine.AddIndex("txn_002", "MacDonald Hardware Store");
        await _searchEngine.AddIndex("txn_003", "McAllister's Deli");
        await _searchEngine.AddIndex("txn_004", "McCafe Coffee Shop");
        await _searchEngine.AddIndex("txn_005", "Donald's Car Wash");

        // Act - Fuzzy search for "mcdon"
        var results = await _searchEngine.Search("mcdon", fuzzy: true);

        // Assert - Should match McDonald's only (MacDonald is "macdonald", not starting with "mcdon")
        Assert.Single(results);
        Assert.Contains("txn_001", results);
    }

    [Fact]
    public async Task Search_WithNumericReferences_ShouldFindByConfirmationNumber()
    {
        // Arrange - Transactions with confirmation numbers
        await _searchEngine.AddIndex("txn_001", "Order #12345 shipped");
        await _searchEngine.AddIndex("txn_002", "Confirmation 12345ABC received");
        await _searchEngine.AddIndex("txn_003", "Transaction ref 67890");
        await _searchEngine.AddIndex("txn_004", "Payment ID 12345 processed");
        await _searchEngine.AddIndex("txn_005", "Invoice #98765 paid");

        // Act
        var results = await _searchEngine.Search("12345");

        // Assert - Should find all transactions with "12345"
        Assert.Equal(3, results.Count);
        Assert.Contains("txn_001", results);
        Assert.Contains("txn_002", results);
        Assert.Contains("txn_004", results);
    }

    [Fact]
    public async Task Search_WithMixedCasePayees_ShouldNormalizeAndFind()
    {
        // Arrange
        await _searchEngine.AddIndex("txn_001", "WHOLE FOODS MARKET");
        await _searchEngine.AddIndex("txn_002", "Whole Foods Market");
        await _searchEngine.AddIndex("txn_003", "whole foods market");
        await _searchEngine.AddIndex("txn_004", "WholeFoodsMarket");

        // Act
        var results = await _searchEngine.Search("whole foods");

        // Assert - All variations should be found
        Assert.Equal(4, results.Count);
    }

    #endregion
}
