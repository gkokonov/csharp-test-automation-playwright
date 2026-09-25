# TypeScript and JavaScript Guide

Prefer TypeScript for examples unless plain JavaScript is requested. Follow the repository's compiler options, module system, lint rules, runtime target, and framework conventions.

This guide covers production code. For tests, suites, and test frameworks read [typescript-testing.md](typescript-testing.md) instead.

## Design choices

- TypeScript is structurally typed: an object satisfies an interface by shape, not declaration. This makes small capability interfaces and function injection natural, but accidental compatibility can hide semantic contract differences.
- Use `interface` for extendable object capabilities and `type` for unions, intersections, aliases, and mapped/conditional forms; choose consistency over dogma when either works.
- Use discriminated unions plus exhaustive checking for a deliberately closed set of variants. Use injected strategies or registration for an open set.
- Functions and object literals are often simpler strategies than classes. Use classes when identity, lifecycle, encapsulated state, or framework integration warrants them.
- `implements` and the TypeScript `private`/`protected` keywords are compile-time constraints. JavaScript `#private` fields also enforce runtime privacy. Validate untrusted runtime data at system boundaries; a type assertion is not validation.
- Avoid mixin or prototype machinery when explicit composition is clearer. JavaScript and TypeScript do not support multiple class inheritance.
- Optional methods can disguise ISP/LSP problems. Prefer separate capability types when clients require different behaviours.

## Complete example: notification composition with a narrow policy contract

The direct version couples orchestration to providers and must change for every channel:

<!-- example: notification-before | verify: compile, run, contract:notification -->
```typescript
type Message = Readonly<{ recipient: string; body: string }>;

class EmailClient {
  sendEmail(message: Message): void {
    console.log(`email to ${message.recipient}: ${message.body}`);
  }
}

class SmsClient {
  sendSms(message: Message): void {
    console.log(`sms to ${message.recipient}: ${message.body}`);
  }
}

class NotificationService {
  private readonly email = new EmailClient();
  private readonly sms = new SmsClient();

  notify(channel: "email" | "sms", message: Message): void {
    if (channel === "email") this.email.sendEmail(message);
    else this.sms.sendSms(message);
  }
}

new NotificationService().notify("email", {
  recipient: "user@example.com",
  body: "Welcome",
});
```

When provider replacement or isolated tests are required, inject one consumer-shaped capability for each existing channel. Preserve the public `notify(channel, message)` method and zero-argument construction:

<!-- example: notification-after | verify: compile, run, contract:notification, contract:injection -->
```typescript
type Message = Readonly<{ recipient: string; body: string }>;

interface Notifier {
  send(message: Message): void;
}

class EmailNotifier implements Notifier {
  send(message: Message): void {
    console.log(`email to ${message.recipient}: ${message.body}`);
  }
}

class SmsNotifier implements Notifier {
  send(message: Message): void {
    console.log(`sms to ${message.recipient}: ${message.body}`);
  }
}

class NotificationService {
  constructor(
    private readonly email: Notifier = new EmailNotifier(),
    private readonly sms: Notifier = new SmsNotifier(),
  ) {}

  notify(channel: "email" | "sms", message: Message): void {
    const notifier = channel === "email" ? this.email : this.sms;
    notifier.send(message);
  }
}

const service = new NotificationService(new EmailNotifier(), new SmsNotifier());

service.notify("email", { recipient: "user@example.com", body: "Welcome" });
```

**Why it is justified:** Each call still sends once through the selected channel, and provider errors propagate to the caller. Injected providers allow replacement and isolated tests. Default providers preserve existing zero-argument construction; this retains concrete wiring in the service as a compatibility trade-off. The fixed two-channel branch is sufficient. Broadcast delivery, a changed constructor contract, and open-ended channel registration are separate changes that require matching requirements.

## Fluent chains and the Law of Demeter

This cohesive builder chain is not automatically a violation because each call remains within one abstraction:

<!-- example: fluent-query | verify: compile, run -->
```typescript
class Query {
  private clauses: string[] = [];

  where(clause: string): this {
    this.clauses.push(clause);
    return this;
  }

  orderBy(field: string): this {
    this.clauses.push(`ORDER BY ${field}`);
    return this;
  }

  build(): string {
    return this.clauses.join(" ");
  }
}

const query = new Query().where("active = true").orderBy("name").build();
console.log(query);
```

By contrast, navigation such as `order.customer.address.country.taxRules.calculate()` exposes several objects' internal structure and creates ripple coupling. Move the outcome to an owning domain/service boundary rather than mechanically wrapping every property.

## Review checks

- Is a type assertion standing in for runtime validation?
- Are optional methods or `throw new Error("not supported")` hiding incompatible capabilities?
- Would a function parameter be simpler than a class plus interface?
- Is a discriminated union intentionally closed, and is handling exhaustive?
- Is a registry/strategy justified by real variants, or is a direct branch clearer today?
- Does structural compatibility also satisfy the semantic contract?
- Is a method chain navigating an object graph or staying within one cohesive fluent API?
