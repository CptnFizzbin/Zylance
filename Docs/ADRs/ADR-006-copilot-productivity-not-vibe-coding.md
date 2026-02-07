# ADR-006: Copilot as Productivity Tool, Not Vibe Coding

## Context

AI-powered coding assistants like GitHub Copilot have become widely available
and can generate substantial amounts of code from prompts or comments. This
raises important questions about how AI should be used in software development:

1. **"Vibe Coding"**: Blindly accepting AI suggestions without understanding or
   reviewing them
2. **Complete rejection**: Refusing to use AI tools due to concerns about
   quality or learning
3. **Thoughtful integration**: Using AI strategically for specific tasks while
   maintaining code quality

As an experimental exploration of AI-assisted development, Zylance needed a
clear philosophy on AI tool usage that:

- Maximizes productivity gains
- Maintains code quality and security
- Preserves developer understanding of the codebase
- Rejects "Vibe Coding™" (blindly accepting AI suggestions)

## Implementation

**Status**: N/A - Development Guideline

## Decision

Use **Copilot as a productivity tool for eliminating tedious busywork**, while 
**hand-coding complex components** and **reviewing all AI-generated code with
meticulous scrutiny**.

This means:

**Use Copilot for:**

- Boilerplate code (DTOs, simple CRUD operations)
- Scaffolding and project structure
- Test fixtures and test data
- Repetitive patterns (similar to existing code)
- Documentation comments
- Simple utility functions
- Rename refactorings (prefer IDE refactorings when possible)

**Hand-code the following:**

- Complex algorithms (QFX parser, fuzzy search)
- Critical business logic
- Security-sensitive code
- Performance-critical paths
- Novel architectural patterns
- Core abstractions and interfaces

**Always review AI-generated code for:**

- Correctness and adherence to requirements
- Performance implications
- Security vulnerabilities
- Code style and consistency
- Proper error handling
- Test coverage
- Documentation accuracy

This is an **experimental exploration** of AI-assisted development that
explicitly rejects "Vibe Coding™"—the practice of blindly accepting AI
suggestions without understanding or scrutiny.

## Consequences

### Positive

- **Productivity boost**: Less time on boilerplate means more time on
  interesting problems
- **Consistency**: AI can maintain consistent patterns across the codebase
- **Learning opportunity**: Experiment with AI-assisted development in a real
  project
- **Reduced tedium**: Less time writing repetitive code
- **Documentation help**: AI can suggest comprehensive doc comments
- **Test generation**: AI can create test scaffolding and edge cases
- **Refactoring assistance**: Large-scale renames and restructuring become
  easier
- **Code exploration**: AI can explain unfamiliar patterns or libraries

### Negative

- **Review burden**: All AI-generated code requires careful review
- **False confidence**: AI-generated code can look correct but have subtle bugs
- **Dependency risk**: Over-reliance on AI could atrophy manual coding skills
- **Security concerns**: AI might suggest insecure patterns. AI usage in coding
  can be perceived as a security risk, especially in the case of a finance app
- **Context limitations**: AI doesn't understand project-specific requirements
- **Quality variance**: AI code quality is inconsistent
- **Learning plateau**: Accepting AI code without understanding limits learning
- **Debugging difficulty**: AI-generated code might be harder to debug

### Mitigations

- Establish clear guidelines on when to use AI vs. hand-coding
- Always understand code before committing, even if AI-generated
- Use AI as a starting point, not the final solution
- Review AI-generated code with extra scrutiny
- Hand-code complex logic to maintain deep understanding
- Use AI for inspiration, not as a source of truth
- Maintain coding skills through deliberate hand-coding of core features
- Run security scanners and tests on all code, regardless of origin

## General Notes

The term "Vibe Coding™" comes from critiques of developers who blindly accept AI
suggestions without understanding what the code does. This leads to:

- Bugs that developers can't debug (because they don't understand the code)
- Security vulnerabilities (because they didn't review critically)
- Degraded problem-solving skills
- Technical debt from code that "works" but isn't maintainable

This ADR is a reaction against that approach. Copilot is a **tool**, not a
replacement for thinking.

**The right mental model**: Copilot is a junior developer who has read a lot of
senior level material. They're knowledgeable about a lot of things, can identify
best practices, but are still *confidently* wrong a lot of the time. You listen
to their suggestions but make the final decisions.

**Real examples from Zylance development:**

**Good Copilot usage:**

- Generated test fixtures for OFX files
- Created boilerplate DTOs for Protocol Buffer messages
- Suggested using source generators (ADR-005)—this was valuable architectural
  input
- Helped scaffold controller structure

**Hand-coded components:**

- QFX/OFX parser (too complex and novel for AI)
- Gateway message routing logic (core architecture)
- Vault provider abstractions (critical interfaces)
- Security-sensitive encryption code (future)

> Stephen:
> 
> **Real-world event**: I had written the initial Raw Elements parser for OFX, and
> while I was away from my computer, I was using Copilot Agents to create a PR to
> process the raw elements into full records for future imports. During the code
> reviews, I was noticing issues, and requested changes to fix them in the PR.
> When I got home and looked at the code in my IDE, I realized that it was
> garbage, and spent the next 4 hours updating the code to be way more
> maintainable (effectively wiping 12h of Copilot requests out). 
> 
> **Lessons learned**:
> provide smaller *well scoped* tasks to agents and don't expect them to fully
> implement features. When creating complex solutions, use an IDE with Copilot
> instead of the GH Agent.

**Common Copilot mistakes:**

- Suggested nullable types where non-null was required
- Generated inefficient LINQ queries (multiple enumerations)
- Used outdated API patterns
- Missed edge cases in error handling
- Suggested libraries that didn't actually exist

The key insight is that **AI is best at generating patterns it has seen before
**. Boilerplate and common patterns are AI's sweet spot. Novel solutions,
complex algorithms, and domain-specific logic require human creativity and
understanding.

This approach also serves as an experiment in AI-assisted development. By being
deliberate about what we use AI for and reviewing everything carefully, we can
learn what works and what doesn't. These learnings will inform how AI tools
should be used in professional software development.

**Philosophical note:** There's a tension between "move fast" and "understand
deeply." AI coding assistants optimize for speed, which is valuable. But speed
without understanding creates fragile systems. The challenge is finding the
right balance—use AI to eliminate toil, but maintain deep understanding of the
system you're building.

> **Stephen:**
>
> My personal stance on AI in Feb 2026: After having a mental breakdown in
> September 2025 around AI (and more specifically AGI), I've been coming to terms
> with it more. Right now, I'm seeing the genuine actual usefulness of Generative
> AI for programming and documentation. Heck, I've been using it to write up these
> ADRs, and implement the code review changes that it and I identify.
>
> I feel like it's helping me be a better developer by assisting with planning,
> identifying edge cases, and prompting me about better solutions. It often
> catches things I've missed when brainstorming ideas, helps with fleshing out my
> own thoughts, and better communication.
>
> It's also *decent* (not great, but decent) at writing documentation for
> functions. They're often too verbose than I'd personally prefer, and often
> documents implementation details instead of high-level explanations of what it
> does.
>
> I also still have this nagging devil on my shoulder that's poking me about "
> Hey... you're just training it to replace you", and I keep having to remind
> myself that this tool is *optional*, I don't *have* to use it, and that even
> though we're throwing orders of magnitude more hardware at these LLMs, they're
> only getting incrementally more effective. A risk I foresee is when the prices
> for these tools inevitability rise and become more expensive than they're worth.
> At the time of writing, GH Copilot is currently C$15/m which so far I feel like
> it is worth it. I might switch to one of Anthropic's offerings in the future,
> but I like the charge by request model that GitHub does instead of the by the
> token model from Anthropic.
>
> An area that I'm largely against the use of GenAI in is the creative
> industries (Art, Movies, Writing, etc...). Pretty much any of the visual arts.
> This is an area where it's humans trying to tell stories and convey emotion to
> other humans. When I look at a painting done by a human, I'm always in awe that
> someone created it, and the mastery needed to make it happen. But when it's done
> by GenAI, the only feeling I get is something hollow.
>
> All in all, it's objectively impressive what this technology can do, but there
> will need to be a balance in what society does with it. Is AGI around the
> corner? I do not think so. LLMs are not the way to get there, but they are a
> stepping stone for sure. I feel like with this boom, a warning shot has been
> fired, and it's up to us a species to plan for if/when AGI does arrive.

---

**For future blog post**: This could be a multi-part blog series:

1. "Vibe Coding vs. Thoughtful AI Usage": Defining the problem
2. "When to Use Copilot (and When Not To)": Practical guidelines
3. "Real Examples from Zylance": War stories and lessons learned
4. "The Future of AI-Assisted Development": Predictions and concerns

The reception of this philosophy could be interesting—some will think it's too
cautious, others will think using AI at all is reckless. The experimental nature
of the project allows for honest reflection on what works.
