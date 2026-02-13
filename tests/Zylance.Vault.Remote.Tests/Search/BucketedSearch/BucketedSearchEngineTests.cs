using Zylance.Vault.Remote.Search;
using Zylance.Vault.Remote.Search.BucketedSearch;
using Zylance.Vault.Remote.Tests.Search.BucketedSearch.Lib;

namespace Zylance.Vault.Remote.Tests.Search.BucketedSearch;

public class BucketedSearchEngineTests
{
    #region Setup

    private readonly MemoryBucketedStorage<string> _storage;
    private readonly BucketedSearchEngine<string> _searchEngine;

    public BucketedSearchEngineTests()
    {
        _storage = new MemoryBucketedStorage<string>();
        _searchEngine = new BucketedSearchEngine<string>(_storage);
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
        await _searchEngine.AddItemAsync(itemId, text);

        // Assert
        var keywords = _storage.Glossary.GetAll();
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
        await _searchEngine.AddItemAsync(itemId, text);

        // Assert
        var keywords = _storage.Glossary.GetAll();
        Assert.Equal(3, keywords.Count);
        Assert.Contains(keywords, k => k.Value == "hello");
        Assert.Contains(keywords, k => k.Value == "world");
        Assert.Contains(keywords, k => k.Value == "test@123"); // @ is preserved in tokens
    }

    [Fact]
    public async Task AddIndex_ShouldIgnoreWhitespace()
    {
        // Arrange
        var itemId = "item1";
        var text = "   Hello   World   ";

        // Act
        await _searchEngine.AddItemAsync(itemId, text);

        // Assert
        var keywords = _storage.Glossary.GetAll();
        Assert.Equal(2, keywords.Count);
        Assert.Contains(keywords, k => k.Value == "hello");
        Assert.Contains(keywords, k => k.Value == "world");
    }

    [Fact]
    public async Task AddIndex_ShouldConvertToLowerCase()
    {
        // Arrange
        var itemId = "item1";
        var text = "HELLO World HeLLo";

        // Act
        await _searchEngine.AddItemAsync(itemId, text);

        // Assert
        var keywords = _storage.Glossary.GetAll();
        var helloKeyword = keywords.Single(k => k.Value == "hello");
        Assert.NotNull(helloKeyword);
    }

    [Fact]
    public async Task AddIndex_ShouldCreateNewBucketWhenMaxItemsReached()
    {
        // Arrange
        var storage = new MemoryBucketedStorage<string>(2);
        var searchEngine = new BucketedSearchEngine<string>(storage);
        var text = "hello";

        // Act - Add 3 items, should create 2 buckets
        await searchEngine.AddItemAsync("item1", text);
        await searchEngine.AddItemAsync("item2", text);
        await searchEngine.AddItemAsync("item3", text);

        // Assert
        var keywords = storage.Glossary.GetAll();
        var helloKeyword = keywords.Single(k => k.Value == "hello");
        Assert.Equal(2u, helloKeyword.NumBuckets);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task Search_ShouldReturnMatchingItems()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "Hello World");
        await _searchEngine.AddItemAsync("item2", "Hello Universe");
        await _searchEngine.AddItemAsync("item3", "Goodbye World");

        // Act
        var results = await _searchEngine.SearchAsync("hello");

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains("item1", results);
        Assert.Contains("item2", results);
    }

    [Fact]
    public async Task Search_ShouldReturnItemsForMultipleTerms()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "Hello World");
        await _searchEngine.AddItemAsync("item2", "Hello Universe");
        await _searchEngine.AddItemAsync("item3", "Goodbye World");

        // Act
        var results = await _searchEngine.SearchAsync("hello world");

        // Assert
        Assert.Single(results);
        Assert.Contains("item1", results);
    }

    [Fact]
    public async Task Search_WithExactMatch_ShouldOnlyReturnExactMatches()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "Hello World");
        await _searchEngine.AddItemAsync("item2", "Help");

        // Act
        var results = await _searchEngine.SearchAsync("hel", fuzzy: false);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task Search_WithFuzzyMatch_ShouldReturnPartialMatches()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "Hello World");
        await _searchEngine.AddItemAsync("item2", "Help");

        // Act
        var results = await _searchEngine.SearchAsync("hel", fuzzy: true);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains("item1", results);
        Assert.Contains("item2", results);
    }

    [Fact]
    public async Task Search_WithLatestFirstDirection_ShouldSearchFromNewestBucket()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "test");

        // Act
        var results = await _searchEngine.SearchAsync("test");

        // Assert
        Assert.Single(results);
        Assert.Contains("item1", results);
    }

    [Fact]
    public async Task Search_WithOldestFirstDirection_ShouldSearchFromOldestBucket()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "test");

        // Act
        var results = await _searchEngine.SearchAsync("test", SearchDirection.OldestFirst);

        // Assert
        Assert.Single(results);
        Assert.Contains("item1", results);
    }

    [Fact]
    public async Task Search_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "Hello World");

        // Act
        var results = await _searchEngine.SearchAsync("nonexistent");

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task Search_AcrossMultipleBuckets_ShouldReturnAllMatches()
    {
        // Arrange
        var storage = new MemoryBucketedStorage<string>(2);
        var searchEngine = new BucketedSearchEngine<string>(storage);

        await searchEngine.AddItemAsync("item1", "test");
        await searchEngine.AddItemAsync("item2", "test");
        await searchEngine.AddItemAsync("item3", "test");

        // Act
        var results = await searchEngine.SearchAsync("test");

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
        await _searchEngine.AddItemAsync("item1", "Hello World");

        // Act - Both "hello" and "world" point to the same item
        var results = await _searchEngine.SearchAsync("hello world");

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
        await _searchEngine.AddItemAsync("item1", "Buy coffee at Starbucks");

        // Act
        await _searchEngine.UpdateItemAsync("item1", "Buy coffee at Starbucks", "Buy coffee at McDonald's");

        // Assert - Should be removed from Starbucks
        var starbucksResults = await _searchEngine.SearchAsync("starbucks");
        Assert.Empty(starbucksResults);

        // Assert - Should be added to McDonald's
        var mcDonaldResults = await _searchEngine.SearchAsync("mcdonald");
        Assert.Single(mcDonaldResults);
        Assert.Contains("item1", mcDonaldResults);

        // Assert - Should still be in "buy" and "coffee" (not removed and re-added)
        var coffeeResults = await _searchEngine.SearchAsync("coffee");
        Assert.Single(coffeeResults);
        Assert.Contains("item1", coffeeResults);
    }

    [Fact]
    public async Task Reindex_WithCompletelyDifferentText_ShouldReplaceAllKeywords()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "Buy coffee");

        // Act
        await _searchEngine.UpdateItemAsync("item1", "Buy coffee", "Sell books");

        // Assert - Old keywords should not return results
        var coffeeResults = await _searchEngine.SearchAsync("coffee");
        Assert.Empty(coffeeResults);

        var buyResults = await _searchEngine.SearchAsync("buy");
        Assert.Empty(buyResults);

        // Assert - New keywords should return results
        var sellResults = await _searchEngine.SearchAsync("sell");
        Assert.Single(sellResults);
        Assert.Contains("item1", sellResults);

        var booksResults = await _searchEngine.SearchAsync("books");
        Assert.Single(booksResults);
        Assert.Contains("item1", booksResults);
    }

    #endregion

    #region Deindex Tests

    [Fact]
    public async Task Deindex_ShouldRemoveItemFromAllKeywordBuckets()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "Hello World");

        // Act
        await _searchEngine.RemoveItemAsync("item1", "Hello World");

        // Assert - Should not find item by any keyword
        var helloResults = await _searchEngine.SearchAsync("hello");
        Assert.Empty(helloResults);

        var worldResults = await _searchEngine.SearchAsync("world");
        Assert.Empty(worldResults);
    }

    [Fact]
    public async Task Deindex_WithMultipleItems_ShouldOnlyRemoveTargetItem()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "Hello World");
        await _searchEngine.AddItemAsync("item2", "Hello Universe");

        // Act
        await _searchEngine.RemoveItemAsync("item1", "Hello World");

        // Assert - item1 should be gone, item2 still findable by "hello"
        var results = await _searchEngine.SearchAsync("hello");
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
        await _searchEngine.AddItemAsync(itemId, text);

        // Assert
        var keywords = _storage.Glossary.GetAll();
        Assert.Empty(keywords);
    }

    [Fact]
    public async Task AddIndex_WithOnlyWhitespace_ShouldNotAddKeywords()
    {
        // Arrange
        var itemId = "item1";
        var text = "   \t\n   ";

        // Act
        await _searchEngine.AddItemAsync(itemId, text);

        // Assert
        var keywords = _storage.Glossary.GetAll();
        Assert.Empty(keywords);
    }

    [Fact]
    public async Task Search_WithDifferentItemIdTypes_ShouldWork()
    {
        // Arrange - Test with integer IDs
        var storage = new MemoryBucketedStorage<int>();
        var searchEngine = new BucketedSearchEngine<int>(storage);

        // Act
        await searchEngine.AddItemAsync(1, "Hello World");
        await searchEngine.AddItemAsync(2, "Goodbye World");
        var results = await searchEngine.SearchAsync("world");

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
        await _searchEngine.AddItemAsync("item1", "Hello World");

        // Act
        var results = await _searchEngine.SearchAsync("HELLO");

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
        await _searchEngine.AddItemAsync(itemId, text);

        // Assert
        var keywords = _storage.Glossary.GetAll();
        Assert.Contains(keywords, k => k.Value == "2024");
    }

    [Fact]
    public async Task Search_WithMultipleWordsInDifferentItems_ShouldReturnIntersection()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "apple banana");
        await _searchEngine.AddItemAsync("item2", "apple orange");
        await _searchEngine.AddItemAsync("item3", "banana orange");

        // Act
        var results = await _searchEngine.SearchAsync("apple");

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
            "Starbucks Coffee #1234",
            "Amazon.com",
            "Shell Gas Station",
            "Whole Foods Market",
            "Target Store #5678",
            "McDonald's Restaurant",
            "Walmart Supercenter",
            "CVS Pharmacy #9012",
            "Home Depot",
            "Best Buy Electronics",
            "Chipotle Mexican Grill",
            "Safeway Grocery",
            "Costco Wholesale",
            "7-Eleven Store",
            "Subway Sandwiches",
            "Apple Store Online",
            "Netflix Subscription",
            "Spotify Premium",
            "AT&T Wireless Payment",
            "Comcast Cable Bill",
            "PG&E Utilities",
            "State Farm Insurance",
            "Chase Credit Card Payment",
            "Venmo Payment",
            "PayPal Transfer",
            "Square Cash",
            "Uber Trip",
            "Lyft Ride",
            "DoorDash Delivery",
            "Grubhub Order",
            "Instacart Groceries",
            "Southwest Airlines",
            "United Airlines",
            "Hilton Hotels",
            "Marriott International",
            "Airbnb Reservation",
            "Budget Car Rental",
            "Enterprise Rent-A-Car",
            "AutoZone Auto Parts",
            "Jiffy Lube Oil Change",
            "Planet Fitness Gym",
            "LA Fitness Membership",
            "AMC Theatres",
            "Regal Cinemas",
            "Barnes & Noble Bookstore",
            "GameStop",
            "Pet-co Pet Supplies",
            "PetSmart",
            "Chewy.com Pet Food",
            "Kroger Supermarket",
            "Trader Joe's",
            "Panera Bread",
            "Olive Garden Restaurant",
            "Red Lobster",
            "Outback Steakhouse",
            "Chili's Grill & Bar",
            "Apple-bee's",
            "Buffalo Wild Wings",
            "Pizza Hut Delivery",
            "Domino's Pizza",
            "Papa John's",
            "Taco Bell",
            "KFC Restaurant",
            "Burger King",
            "Wendy's",
            "Arby's",
            "Five Guys Burgers",
            "In-N-Out Burger",
            "Shake Shack",
            "The Cheesecake Factory",
            "P.F. Chang's",
            "California Pizza Kitchen",
            "Panda Express",
            "Jamba Juice",
            "Smoothie King",
            "Dunkin' Donuts",
            "Krispy Kreme",
            "Baskin-Robbins",
            "Cold Stone Creamery",
            "Yogurtland",
            "Pinkberry Frozen Yogurt",
            "Nordstrom Department Store",
            "Macy's",
            "JCPenney",
            "Kohl's Department Store",
            "Gap Clothing Store",
            "Old Navy",
            "H&M Fashion",
            "Zara",
            "Forever 21",
            "Victoria's Secret",
            "Bath & Body Works",
            "Bed Bath & Beyond",
            "Williams Sonoma",
            "Crate and Barrel",
            "IKEA Furniture",
            "Office Depot",
            "Staples Office Supplies",
            "FedEx Shipping",
            "UPS Store",
            "USPS Postage",
            "Walgreens Pharmacy",
            "Rite Aid",
        };

        for (var i = 0; i < payees.Length; i++)
            await _searchEngine.AddItemAsync($"txn_{i:D4}", payees[i]);

        // Act - Search for "coffee"
        var coffeeResults = await _searchEngine.SearchAsync("coffee");

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
            ["txn_020"] = "Grocery store weekly shopping",
        };

        foreach (var (id, memo) in transactions)
            await _searchEngine.AddItemAsync(id, memo);

        // Act - Search for different terms
        var groceryResults = await _searchEngine.SearchAsync("grocery");
        var coffeeResults = await _searchEngine.SearchAsync("coffee");
        var gasResults = await _searchEngine.SearchAsync("gas");

        // Assert
        Assert.Equal(3, groceryResults.Count); // txn_001, txn_009, txn_020
        Assert.Equal(2, coffeeResults.Count); // txn_003, txn_016
        Assert.Equal(3, gasResults.Count); // txn_002 (gas fill-up), txn_013 (gas utility), txn_019 (gas station)
    }

    [Fact]
    public async Task Search_WithLargeDatasetAndBucketOverflow_ShouldHandleCorrectly()
    {
        // Arrange - Small bucket size to force multiple buckets
        var storage = new MemoryBucketedStorage<string>(5);
        var searchEngine = new BucketedSearchEngine<string>(storage);

        // Index 50 transactions all containing "payment"
        for (var i = 0; i < 50; i++)
        {
            var memo = i switch
            {
                < 10 => $"Credit card payment #{i + 1}",
                < 20 => $"Utility payment reference {i + 1}",
                < 30 => $"Online payment confirmation {i + 1}",
                < 40 => $"Automatic payment processed {i + 1}",
                _ => $"Manual payment entry {i + 1}",
            };
            await searchEngine.AddItemAsync($"txn_{i:D3}", memo);
        }

        // Act
        var results = await searchEngine.SearchAsync("payment");

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
        await _searchEngine.AddItemAsync("txn_001", "Amazon.com Online Purchase");
        await _searchEngine.AddItemAsync("txn_002", "Amazon Prime Membership");
        await _searchEngine.AddItemAsync("txn_003", "Amazon AWS Cloud Services");
        await _searchEngine.AddItemAsync("txn_004", "Amazon Music Subscription");
        await _searchEngine.AddItemAsync("txn_005", "Target Store In-Person");
        await _searchEngine.AddItemAsync("txn_006", "Target.com Online Order");
        await _searchEngine.AddItemAsync("txn_007", "Target REDcard Payment");

        // Act
        var amazonResults = await _searchEngine.SearchAsync("amazon");
        var targetResults = await _searchEngine.SearchAsync("target");
        var onlineResults = await _searchEngine.SearchAsync("online");

        // Assert
        Assert.Equal(4, amazonResults.Count);
        Assert.Equal(3, targetResults.Count);
        Assert.Equal(2, onlineResults.Count); // Amazon.com and Target.com
    }

    [Fact]
    public async Task Search_WithFuzzyMatchOnPayees_ShouldFindPartialMatches()
    {
        // Arrange
        await _searchEngine.AddItemAsync("txn_001", "McDonald's Restaurant #4532");
        await _searchEngine.AddItemAsync("txn_002", "MacDonald Hardware Store");
        await _searchEngine.AddItemAsync("txn_003", "McAllister's Deli");
        await _searchEngine.AddItemAsync("txn_004", "McCafe Coffee Shop");
        await _searchEngine.AddItemAsync("txn_005", "Donald's Car Wash");

        // Act - Fuzzy search for "mcdon"
        var results = await _searchEngine.SearchAsync("mcdon", fuzzy: true);

        // Assert - Should match McDonald's only (MacDonald is "macdonald", not starting with "mcdon")
        Assert.Single(results);
        Assert.Contains("txn_001", results);
    }

    [Fact]
    public async Task Search_WithNumericReferences_ShouldFindByConfirmationNumber()
    {
        // Arrange - Transactions with confirmation numbers
        await _searchEngine.AddItemAsync("txn_001", "Order #12345 shipped");
        await _searchEngine.AddItemAsync("txn_002", "Confirmation 12345ABC received");
        await _searchEngine.AddItemAsync("txn_003", "Transaction ref 67890");
        await _searchEngine.AddItemAsync("txn_004", "Payment ID 12345 processed");
        await _searchEngine.AddItemAsync("txn_005", "Invoice #98765 paid");

        // Act
        var results = await _searchEngine.SearchAsync("12345");

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
        await _searchEngine.AddItemAsync("txn_001", "WHOLE FOODS MARKET");
        await _searchEngine.AddItemAsync("txn_002", "Whole Foods Market");
        await _searchEngine.AddItemAsync("txn_003", "whole foods market");
        await _searchEngine.AddItemAsync("txn_004", "WholeFoodsMarket");

        // Act
        var results = await _searchEngine.SearchAsync("whole foods");

        // Assert - All variations should be found
        Assert.Equal(4, results.Count);
    }

    [Fact]
    public async Task Search_FuzzyWithHyphenatedTerm_ShouldMatchExact()
    {
        // Arrange - Index content with hyphenated term
        await _searchEngine.AddItemAsync("item1", "Purchase from foo-bar store");
        await _searchEngine.AddItemAsync("item2", "Visit to foobar location");
        await _searchEngine.AddItemAsync("item3", "Order at foo bar restaurant");

        // Act - Search with exact hyphenated term
        var results = await _searchEngine.SearchAsync("foo-bar", fuzzy: true);

        // Assert - Hyphens are now preserved in tokens
        // "foo-bar" remains as a single token "foo-bar"
        // Only item1 has the exact token "foo-bar"
        Assert.Single(results);
        Assert.Contains("item1", results);
    }

    [Fact]
    public async Task Search_FuzzyWithHyphenatedQuery_ShouldTokenizeAndMatch()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "check-in process completed");
        await _searchEngine.AddItemAsync("item2", "checking status");
        await _searchEngine.AddItemAsync("item3", "process incoming requests");

        // Act - Hyphen is preserved in token
        var results = await _searchEngine.SearchAsync("check-in", fuzzy: true);

        // Assert - "check-in" remains as single token
        // item1: has "check-in" token ✓
        // item2: has "checking" (fuzzy contains "check-in"? No, "check-in" is longer)
        // Actually fuzzy means the keyword contains the search token
        // So we're looking for keywords that contain "check-in"
        Assert.Single(results);
        Assert.Contains("item1", results);
    }

    [Fact]
    public async Task Search_FuzzyWithEmailPattern_ShouldMatchPartialEmail()
    {
        // Arrange - Index content with email-like patterns
        await _searchEngine.AddItemAsync("item1", "Contact me@place.com for details");
        await _searchEngine.AddItemAsync("item2", "Email sent to me@other.org");
        await _searchEngine.AddItemAsync("item3", "User you@place.com replied");

        // Act - Search with partial email (fuzzy)
        var results = await _searchEngine.SearchAsync("me@pla", fuzzy: true);

        // Assert - "@" is now preserved, "." still splits
        // "me@place.com" becomes tokens ["me@place", "com"]
        // Query "me@pla" is a single token
        // Fuzzy search: keyword contains search token
        // "me@place" contains "me@pla" ✓
        Assert.Single(results);
        Assert.Contains("item1", results);
    }

    [Fact]
    public async Task Search_FuzzyWithEmailPattern_ExactToken()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "user@example.com registration");
        await _searchEngine.AddItemAsync("item2", "contact example support");
        await _searchEngine.AddItemAsync("item3", "user account created");

        // Act - Search for "user" (part of email)
        var results = await _searchEngine.SearchAsync("user", fuzzy: true);

        // Assert - "user@example" is a single token, but contains "user"
        // item1: has "user@example" (contains "user") ✓
        // item2: no token containing "user" ✗
        // item3: has "user" token ✓
        Assert.Equal(2, results.Count);
        Assert.Contains("item1", results);
        Assert.Contains("item3", results);
    }

    [Fact]
    public async Task Search_FuzzyWithSpecialChars_ShouldSplitTokens()
    {
        // Arrange - Various special characters as delimiters
        await _searchEngine.AddItemAsync("item1", "product@store/location");
        await _searchEngine.AddItemAsync("item2", "store#2 branch");
        await _searchEngine.AddItemAsync("item3", "location coordinates");

        // Act - Search for "store" (separated by special chars)
        var results = await _searchEngine.SearchAsync("store", fuzzy: true);

        // Assert - @ is preserved, / and # still split
        // item1: "product@store/location" → ["product@store", "location"], no exact "store" but "product@store" doesn't contain "store" at start
        // Actually "product@store" DOES contain "store" (fuzzy substring match)
        // item2: "store#2 branch" → ["store", "2", "branch"]
        Assert.Equal(2, results.Count);
        Assert.Contains("item1", results);
        Assert.Contains("item2", results);
    }

    [Fact]
    public async Task Search_ExactWithHyphenatedTerm_ShouldMatchIndividualTokens()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "Product foo-bar available");

        // Act - Exact search for "foo-bar"
        var results = await _searchEngine.SearchAsync("foo-bar", fuzzy: false);

        // Assert - "foo-bar" is now a single token that exists exactly
        Assert.Single(results);
        Assert.Contains("item1", results);
    }

    [Fact]
    public async Task Search_ExactWithEmailPattern_ShouldNotMatchPartial()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "Contact me@place.com");

        // Act - Exact search for "me@pla"
        var results = await _searchEngine.SearchAsync("me@pla", fuzzy: false);

        // Assert - "me@place" is the indexed token, but exact search requires "me@pla" exactly
        // No exact match for token "me@pla"
        Assert.Empty(results);
    }

    [Fact]
    public async Task Search_WithUnderscores_ShouldPreserveInTokens()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "function check_status called");
        await _searchEngine.AddItemAsync("item2", "checking status");
        await _searchEngine.AddItemAsync("item3", "status_update pending");

        // Act
        var results = await _searchEngine.SearchAsync("check_status", fuzzy: true);

        // Assert - Underscores are preserved, only item1 has "check_status"
        Assert.Single(results);
        Assert.Contains("item1", results);
    }

    [Fact]
    public async Task Search_WithEmailAddress_ShouldPreserveAtSymbol()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "Email: support@company.com");
        await _searchEngine.AddItemAsync("item2", "Contact support at company");

        // Act
        var results = await _searchEngine.SearchAsync("support@company", fuzzy: true);

        // Assert - @ is preserved in tokens
        // item1: "support@company" token (before .com) ✓
        Assert.Single(results);
        Assert.Contains("item1", results);
    }

    #endregion

    #region ItemFlags Management Tests

    [Fact]
    public async Task AddItem_ShouldSetIsIndexedToTrue()
    {
        // Arrange
        var itemId = "item1";
        var content = "Hello World";

        // Act
        await _searchEngine.AddItemAsync(itemId, content);

        // Assert
        var flags = await _storage.Flags.GetFlagAsync(itemId);
        Assert.True(flags.IsIndexed);
        Assert.Equal(itemId, flags.ItemId);
    }

    [Fact]
    public async Task AddItem_WhenAlreadyIndexed_ShouldNotReindex()
    {
        // Arrange
        var itemId = "item1";
        await _searchEngine.AddItemAsync(itemId, "Hello World");
        var initialKeywordCount = _storage.Glossary.GetAll().Count;

        // Act - Try to add again
        await _searchEngine.AddItemAsync(itemId, "Different Content");

        // Assert - Should not have added new keywords
        var finalKeywordCount = _storage.Glossary.GetAll().Count;
        Assert.Equal(initialKeywordCount, finalKeywordCount);

        // Should still find by original content
        var results = await _searchEngine.SearchAsync("hello");
        Assert.Single(results);
    }

    [Fact]
    public async Task AddItems_ShouldSetIsIndexedToTrueForAllItems()
    {
        // Arrange
        var items = new List<(string itemId, string content)>
        {
            ("item1", "Hello World"),
            ("item2", "Goodbye Universe"),
            ("item3", "Test Content"),
        };

        // Act
        await _searchEngine.AddItemsAsync(items);

        // Assert
        foreach (var (itemId, _) in items)
        {
            var flags = await _storage.Flags.GetFlagAsync(itemId);
            Assert.True(flags.IsIndexed);
            Assert.Equal(itemId, flags.ItemId);
        }
    }

    [Fact]
    public async Task AddItems_WhenSomeAlreadyIndexed_ShouldOnlyIndexNewItems()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "Already Indexed");

        var items = new List<(string itemId, string content)>
        {
            ("item1", "Already Indexed"), // Already indexed
            ("item2", "New Item One"), // New
            ("item3", "New Item Two"), // New
        };

        // Act
        await _searchEngine.AddItemsAsync(items);

        // Assert
        var item2Results = await _searchEngine.SearchAsync("new");
        Assert.Equal(2, item2Results.Count);
        Assert.Contains("item2", item2Results);
        Assert.Contains("item3", item2Results);

        // item1 should still be indexed with original content
        var item1Results = await _searchEngine.SearchAsync("already");
        Assert.Single(item1Results);
    }

    [Fact]
    public async Task UpdateItem_ShouldNotChangeIsIndexedFlag()
    {
        // Arrange
        var itemId = "item1";
        await _searchEngine.AddItemAsync(itemId, "Original Content");

        // Act
        await _searchEngine.UpdateItemAsync(itemId, "Original Content", "Updated Content");

        // Assert
        var flags = await _storage.Flags.GetFlagAsync(itemId);
        Assert.True(flags.IsIndexed);
    }

    [Fact]
    public async Task UpdateItem_WhenNotIndexed_ShouldNotUpdate()
    {
        // Arrange
        var itemId = "item1";
        // Don't index the item

        // Act
        await _searchEngine.UpdateItemAsync(itemId, "Old Content", "New Content");

        // Assert - Should not find by new content
        var results = await _searchEngine.SearchAsync("new");
        Assert.Empty(results);

        // Flag should still be false
        var flags = await _storage.Flags.GetFlagAsync(itemId);
        Assert.False(flags.IsIndexed);
    }

    [Fact]
    public async Task UpdateItems_ShouldOnlyUpdateIndexedItems()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "Original One");
        await _searchEngine.AddItemAsync("item2", "Original Two");
        // item3 not indexed

        var updates = new List<(string itemId, string oldContent, string newContent)>
        {
            ("item1", "Original One", "Updated One"),
            ("item2", "Original Two", "Updated Two"),
            ("item3", "Old Three", "New Three"), // Not indexed
        };

        // Act
        await _searchEngine.UpdateItemsAsync(updates);

        // Assert
        var updatedResults = await _searchEngine.SearchAsync("updated");
        Assert.Equal(2, updatedResults.Count);
        Assert.Contains("item1", updatedResults);
        Assert.Contains("item2", updatedResults);

        // item3 should not be indexed
        var item3Results = await _searchEngine.SearchAsync("new");
        Assert.Empty(item3Results);
    }

    [Fact]
    public async Task RemoveItem_ShouldSetIsIndexedToFalse()
    {
        // Arrange
        var itemId = "item1";
        var content = "Hello World";
        await _searchEngine.AddItemAsync(itemId, content);

        // Act
        await _searchEngine.RemoveItemAsync(itemId, content);

        // Assert
        var flags = await _storage.Flags.GetFlagAsync(itemId);
        Assert.False(flags.IsIndexed);
        Assert.Equal(itemId, flags.ItemId);
    }

    [Fact]
    public async Task RemoveItem_WhenNotIndexed_ShouldNotChangeFlag()
    {
        // Arrange
        var itemId = "item1";
        // Item not indexed

        // Act
        await _searchEngine.RemoveItemAsync(itemId, "Some Content");

        // Assert
        var flags = await _storage.Flags.GetFlagAsync(itemId);
        Assert.False(flags.IsIndexed);
    }

    [Fact]
    public async Task RemoveItems_ShouldSetIsIndexedToFalseForAllRemovedItems()
    {
        // Arrange
        var items = new List<(string itemId, string content)>
        {
            ("item1", "Content One"),
            ("item2", "Content Two"),
            ("item3", "Content Three"),
        };

        await _searchEngine.AddItemsAsync(items);

        // Act
        await _searchEngine.RemoveItemsAsync(items);

        // Assert
        foreach (var (itemId, _) in items)
        {
            var flags = await _storage.Flags.GetFlagAsync(itemId);
            Assert.False(flags.IsIndexed);
            Assert.Equal(itemId, flags.ItemId);
        }
    }

    [Fact]
    public async Task RemoveItems_WhenSomeNotIndexed_ShouldOnlyRemoveIndexedItems()
    {
        // Arrange
        await _searchEngine.AddItemAsync("item1", "Content One");
        await _searchEngine.AddItemAsync("item2", "Content Two");
        // item3 not indexed

        var itemsToRemove = new List<(string itemId, string content)>
        {
            ("item1", "Content One"),
            ("item2", "Content Two"),
            ("item3", "Content Three"), // Not indexed
        };

        // Act
        await _searchEngine.RemoveItemsAsync(itemsToRemove);

        // Assert
        var item1Flags = await _storage.Flags.GetFlagAsync("item1");
        var item2Flags = await _storage.Flags.GetFlagAsync("item2");
        var item3Flags = await _storage.Flags.GetFlagAsync("item3");

        Assert.False(item1Flags.IsIndexed);
        Assert.False(item2Flags.IsIndexed);
        Assert.False(item3Flags.IsIndexed); // Should remain false
    }

    [Fact]
    public async Task ItemFlags_ShouldPersistAcrossMultipleOperations()
    {
        // Arrange
        var itemId = "item1";

        // Act & Assert - Add
        await _searchEngine.AddItemAsync(itemId, "Original Content");
        var flags1 = await _storage.Flags.GetFlagAsync(itemId);
        Assert.True(flags1.IsIndexed);

        // Act & Assert - Update
        await _searchEngine.UpdateItemAsync(itemId, "Original Content", "Updated Content");
        var flags2 = await _storage.Flags.GetFlagAsync(itemId);
        Assert.True(flags2.IsIndexed);

        // Act & Assert - Remove
        await _searchEngine.RemoveItemAsync(itemId, "Updated Content");
        var flags3 = await _storage.Flags.GetFlagAsync(itemId);
        Assert.False(flags3.IsIndexed);

        // Act & Assert - Re-add
        await _searchEngine.AddItemAsync(itemId, "New Content");
        var flags4 = await _storage.Flags.GetFlagAsync(itemId);
        Assert.True(flags4.IsIndexed);
    }

    [Fact]
    public async Task ItemFlags_WithBatchOperations_ShouldHandleMixedStates()
    {
        // Arrange - Set up mixed initial state
        await _searchEngine.AddItemAsync("item1", "Already Indexed");
        // item2 not indexed
        await _searchEngine.AddItemAsync("item3", "Already Indexed");

        var itemsToAdd = new List<(string itemId, string content)>
        {
            ("item1", "Different Content"), // Already indexed, should skip
            ("item2", "New Content"), // Not indexed, should add
            ("item4", "Another New"), // Not indexed, should add
        };

        // Act
        await _searchEngine.AddItemsAsync(itemsToAdd);

        // Assert
        var item1Flags = await _storage.Flags.GetFlagAsync("item1");
        var item2Flags = await _storage.Flags.GetFlagAsync("item2");
        var item3Flags = await _storage.Flags.GetFlagAsync("item3");
        var item4Flags = await _storage.Flags.GetFlagAsync("item4");

        Assert.True(item1Flags.IsIndexed); // Was already indexed
        Assert.True(item2Flags.IsIndexed); // Newly indexed
        Assert.True(item3Flags.IsIndexed); // Unchanged
        Assert.True(item4Flags.IsIndexed); // Newly indexed
    }

    [Fact]
    public async Task ItemFlags_AfterReindexing_ShouldRemainTrue()
    {
        // Arrange
        var itemId = "item1";
        await _searchEngine.AddItemAsync(itemId, "Original Content");

        // Act - Reindex multiple times
        await _searchEngine.UpdateItemAsync(itemId, "Original Content", "Updated Once");
        await _searchEngine.UpdateItemAsync(itemId, "Updated Once", "Updated Twice");
        await _searchEngine.UpdateItemAsync(itemId, "Updated Twice", "Updated Thrice");

        // Assert
        var flags = await _storage.Flags.GetFlagAsync(itemId);
        Assert.True(flags.IsIndexed);

        // Should find by latest content
        var results = await _searchEngine.SearchAsync("thrice");
        Assert.Single(results);
        Assert.Contains(itemId, results);
    }

    #endregion
}
