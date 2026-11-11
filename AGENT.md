# AGENT.md

Behavioral guidelines to reduce common LLM coding mistakes. Merge with project-specific instructions as needed.

## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

## 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

## 5. Agentic Skills

**Use the harness deliberately. Prefer evidence over guesswork.**

- **Plan** — Turn multi-step work into steps, each with a verification check.
- **Explore** — Search and read the relevant code before editing; never guess at file contents.
- **Implement** — Make the smallest change that satisfies the request; prefer targeted edits.
- **Verify** — Run the real build and tests after every change, and report the actual output.
- **Debug** — Reproduce the failure first, fix the cause, then add a regression test.
- **Docker** — Use `docker compose up --build` for the stack and
  `docker compose --profile test run --rm --build backend-tests` for the backend suite.
- **Document** — Keep `README.md`, `ASSUMPTIONS.md`, and `AI_USAGE.md` in sync with behavior.
- **BRD fidelity** — Match the contract exactly: endpoint paths, field names, status casing,
  UI labels, ports, and environment variables.
- **Report** — Surface failures and tradeoffs plainly; never claim a check passed without
  running it.

## 6. Local Commits

**Commit as you go. Keep every commit small and self-explanatory.**

After completing a coherent change, commit it to git **locally** (do not push unless asked):

- **One concern per commit.** Split unrelated edits into separate commits; never bundle
  several features, refactors, or a code change plus unrelated docs into one commit.
- **Conventional Commits format** for the subject line:
  `type(scope): imperative summary` — for example `fix(backend): reject seats above the limit`.
  Common types: `feat`, `fix`, `refactor`, `test`, `docs`, `style`, `chore`, `perf`, `build`.
  Keep the subject under ~72 characters, lowercase, no trailing period.
- **Explain what and why in the body.** Say what changed, why it was needed, and any
  tradeoff or follow-up. The diff already shows *how*; the message must show *intent*.
- **Verify before committing.** Only commit once the relevant build/tests pass, and never
  commit secrets, generated artifacts, or unrelated working-tree changes.
- **Report each commit** (hash + subject) in the final response so the user can review it.