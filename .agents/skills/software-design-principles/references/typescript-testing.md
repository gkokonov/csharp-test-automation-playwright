# TypeScript Test Automation Guide

Cross-language principles live in [Test Automation Design](test-automation.md); production-code design for TypeScript lives in [typescript.md](typescript.md). This guide covers Playwright Test.

## Contents

- [Design choices](#design-choices)
- [Fixture example](#illustrative-example-module-scoped-state-and-hidden-assertions-to-fixtures)
- [Review checks](#review-checks)

## Design choices

- Playwright Test's fixtures are its composition mechanism. `test.extend<Fixtures>({ ... })` adds per-concern setup that a spec requests by destructuring; there is no reason to build a base test class.
- The built-in `page` fixture is already per test. Creating a browser or page at module scope reintroduces the shared state fixtures exist to avoid.
- Declare `{ scope: "worker" }` only for expensive immutable setup. Worker-scoped mutable state is shared across every test that worker runs.
- Prefer `getByRole`, `getByLabel`, and `getByTestId` over CSS or XPath. Web-first assertions such as `await expect(locator).toHaveText(...)` retry until the timeout, so `page.waitForTimeout` is nearly always a design smell.
- Returning a `Locator` from a page object for `expect()` keeps the selector encapsulated, because a locator is a lazy query re-resolved on each use. Returning an `ElementHandle` pins one DOM node and goes stale.
- Use the `request` fixture or an `APIRequestContext` inside a client that owns request/auth plumbing. Return typed objects for domain scenarios; expose status, headers, and JSON values when they are the contract under test. Follow the repository's established client return type.
- `test.step` improves failure diagnosis for long flows without extracting the narrative into helpers—it is the DAMP-friendly way to add structure.
- Avoid `test.describe.serial` unless the order is genuinely required; it removes isolation and retries the whole block on one failure.
- Build test data with factory functions taking a partial override, so each spec states only the fields it depends on.

## Illustrative example: module-scoped state and hidden assertions to fixtures

The starting point shares a page at module scope, hides the expectation inside a helper, and sleeps:

<!-- example: playwright-before | verify: none (requires @playwright/test) -->
```typescript
import { test, expect, chromium, Browser, Page } from "@playwright/test";

let browser: Browser;
let page: Page;

test.beforeAll(async () => {
  browser = await chromium.launch();
  page = await browser.newPage();
});

async function createOrderAndVerify(name: string, expectToast = true) {
  await page.fill("#customer", name);
  await page.click(".btn-primary");
  await page.waitForTimeout(2000);
  if (expectToast) {
    expect(await page.textContent(".toast")).toBe("Order created");
  }
}

test("creates order", async () => {
  await createOrderAndVerify("Ada");
});
```

Scope state per test with a fixture, encapsulate the selectors, and return the expectation to the spec:

<!-- example: playwright-after | verify: none (requires @playwright/test) -->
```typescript
import { test as base, expect, type Page, type Locator } from "@playwright/test";

type Customer = Readonly<{ name: string; tier: string }>;

const aCustomer = (overrides: Partial<Customer> = {}): Customer => ({
  name: "Ada",
  tier: "standard",
  ...overrides,
});

class OrdersPage {
  constructor(private readonly page: Page) {}

  private get customerName(): Locator {
    return this.page.getByLabel("Customer");
  }

  private get submit(): Locator {
    return this.page.getByRole("button", { name: "Create order" });
  }

  get confirmation(): Locator {
    return this.page.getByTestId("order-toast");
  }

  async createOrderFor(customer: Customer): Promise<void> {
    await this.customerName.fill(customer.name);
    await this.submit.click();
  }
}

const test = base.extend<{ orders: OrdersPage }>({
  orders: async ({ page }, use) => {
    await page.goto("/orders");
    await use(new OrdersPage(page));
  },
});

test("shows a confirmation after creating an order", async ({ orders }) => {
  await orders.createOrderFor(aCustomer({ name: "Ada" }));

  await expect(orders.confirmation).toHaveText("Order created");
});
```

**Why it is justified:** The fixture gives each test its own page, so the suite parallelises and no module-scoped state leaks between specs. `OrdersPage` owns the selectors while the expected text stays in the spec, which is the deliberate DAMP trade: the reader of a failure sees `"Order created"` without opening another file. The retrying `expect` replaces the fixed sleep. The `expectToast` flag disappeared—a boolean that switched a helper's behaviour per caller was the signal that the helper was doing two jobs.

## Review checks

- Is a browser, context, or page created at module scope instead of through a fixture?
- Is any worker-scoped fixture holding mutable state?
- Does a spec contain CSS/XPath strings, `page.waitForTimeout`, or a manual retry loop?
- Are assertions hidden inside helpers, leaving the spec with no visible expectation?
- Does a helper take boolean or enum flags that select behaviour per caller?
- Does a spec repeat request/auth plumbing or depend on wire-format details outside the contract it is meant to verify?
- Is `test.describe.serial` used where isolation was simply inconvenient?
- Are page objects returning `ElementHandle` (stale-prone) where a `Locator` (lazy) belongs?
