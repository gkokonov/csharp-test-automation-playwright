# Java Test Automation Guide

Cross-language principles live in [Test Automation Design](test-automation.md); production-code design for Java lives in [java.md](java.md). This guide covers JUnit 5, Playwright for Java, and REST Assured.

## Contents

- [Design choices](#design-choices)
- [Fixture example](#illustrative-example-shared-static-state-to-per-test-composition)
- [Review checks](#review-checks)

## Design choices

- JUnit 5 favours composition over a base class: `@RegisterExtension` and custom `Extension` implementations, parameter resolution, and `@TestInstance` lifecycle give per-concern setup without inheritance. Reach for an extension before another `AbstractBaseTest` layer.
- `@BeforeEach` is per test; `@BeforeAll` is per class and static. Under `junit.jupiter.execution.parallel.enabled`, `@BeforeAll` state is shared across concurrent tests and must be immutable or explicitly synchronized. `@ResourceLock` declares contention instead of hiding it.
- Playwright: build `Page` per test from a `BrowserContext` per test. A `static Page` or `static Playwright` shared across tests defeats parallel execution and leaks state between specs.
- Prefer `getByRole`, `getByLabel`, and `getByTestId` to CSS or XPath strings. Playwright's `Locator` auto-waits on observable state, so `page.waitForTimeout` is nearly always a design smell. Exposing a `Locator` for `assertThat(locator)` still encapsulates the selector, because a locator is a lazy re-resolving query; an `ElementHandle` pins one node and goes stale.
- Keep REST Assured request construction, auth, and shared `RequestSpecification` plumbing in the client. Return typed objects via `.as(Order.class)` for domain scenarios; expose response metadata and wire-format values when the HTTP contract is under test. Follow the repository's client contract rather than forbidding transport assertions.
- Prefer `record` for test data plus a builder with defaults; override only the fields under test.
- Use AssertJ `assertThat` with `as(...)` descriptions, or `assertAll` for several assertions describing one outcome, so a failure names the concern.

## Illustrative example: shared static state to per-test composition

The starting point shares a static page, hardcodes selectors in the spec, sleeps, and asserts on the wire format:

<!-- example: junit-before | verify: none (requires JUnit 5, Playwright, REST Assured) -->
```java
abstract class AbstractOrderTest {
    protected static Page page;
    protected static String authToken;

    @BeforeAll
    static void setUpAll() {
        Playwright playwright = Playwright.create();
        Browser browser = playwright.chromium().launch();
        page = browser.newPage();
        RestAssured.baseURI = "https://api.example.com";
        authToken = logIn("standard_user", "secret");
    }
}

class OrderTest extends AbstractOrderTest {
    @Test
    void createsOrder() throws InterruptedException {
        page.fill("#customer", "Ada");
        page.click(".btn-primary");
        Thread.sleep(2000);
        assertEquals("Order created", page.textContent(".toast"));

        RestAssured.given().header("Authorization", "Bearer " + authToken)
            .when().get("/orders/3")
            .then().body("data.attributes.customerName", equalTo("Ada"));
    }
}
```

Scope the browser per test, encapsulate selectors and transport, and keep the expectation in the spec:

<!-- example: junit-after | verify: none (requires JUnit 5, Playwright, REST Assured) -->
```java
record Customer(String name, String tier) {
    static Customer named(String name) {
        return new Customer(name, "standard");
    }
}

final class OrdersPage {
    private final Page page;

    OrdersPage(Page page) {
        this.page = page;
    }

    private Locator customerName() {
        return page.getByLabel("Customer");
    }

    private Locator submit() {
        return page.getByRole(AriaRole.BUTTON,
            new Page.GetByRoleOptions().setName("Create order"));
    }

    Locator confirmation() {
        return page.getByTestId("order-toast");
    }

    void createOrderFor(Customer customer) {
        customerName().fill(customer.name());
        submit().click();
    }
}

final class OrdersApi {
    private final RequestSpecification spec;

    OrdersApi(String baseUri, String token) {
        this.spec = new RequestSpecBuilder()
            .setBaseUri(baseUri)
            .addHeader("Authorization", "Bearer " + token)
            .build();
    }

    Order create(Customer customer) {
        return RestAssured.given().spec(spec).contentType(ContentType.JSON)
            .body(customer)
            .when().post("/orders")
            .then().statusCode(201)
            .extract().as(Order.class);
    }
}

@Execution(ExecutionMode.CONCURRENT)
class OrderTest {
    @RegisterExtension
    final BrowserPageExtension browserPage = new BrowserPageExtension();

    @Test
    void showsConfirmationAfterCreatingAnOrder() {
        OrdersPage orders = new OrdersPage(browserPage.page());
        browserPage.page().navigate("/orders");

        orders.createOrderFor(Customer.named("Ada"));

        assertThat(orders.confirmation()).hasText("Order created");
    }
}
```

**Why it is justified:** The extension owns browser lifecycle per test, so concurrent execution is safe and no `static` state leaks between specs. `OrdersPage` holds the selectors; the expected text stays in the spec where a failure is read. Playwright's auto-retrying `assertThat` replaces `Thread.sleep`. `OrdersApi` confines the REST Assured chain and returns a typed `Order`, so specs stop depending on JSON paths.

## Review checks

- Is `Page`, `Browser`, or `Playwright` held in a `static` or `@BeforeAll` field that tests mutate?
- Does a spec contain CSS/XPath strings, `Thread.sleep`, or `page.waitForTimeout`?
- Do specs repeat REST Assured request/auth plumbing? Are response metadata and JSON-path assertions required by the contract under test?
- Does an `AbstractBaseTest` own more than framework lifecycle, and would a `@RegisterExtension` extension replace it?
- Is `@BeforeAll` state immutable, or does parallel execution share mutable setup without `@ResourceLock`?
- Do assertions carry `as(...)` descriptions, or would a failure be anonymous?
- Are page objects returning `ElementHandle` where a `Locator` belongs?
- Does each test build its own data with unique identifiers where it touches shared state?
