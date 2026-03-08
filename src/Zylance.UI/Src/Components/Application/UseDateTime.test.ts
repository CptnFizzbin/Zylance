import { describe, expect, test, vi } from "vitest"
import { useTimestamp } from "@/Components/Application/UseDateTime"

// Mutable format object used by the mock so each test can set patterns
let currentFormat: { datePattern?: string; timePattern?: string } = {
  datePattern: "yyyy-MM-dd",
  timePattern: "HH:mm:ss",
}

vi.mock("@/Components/Application/UseZylance", () => ({
  useZylance: () => ({
    settings: {
      userPreferences: {
        dateTimeFormat: currentFormat,
      },
    },
  }),
}))

describe("useTimestamp", () => {
  const ts = new Date("2026-03-07T09:08:07")

  test("it returns the full timestamp by default", () => {
    currentFormat = { datePattern: "yyyy-MM-dd", timePattern: "HH:mm:ss" }
    const result = useTimestamp(ts)
    expect(result).toBe("2026-03-07 09:08:07")
  })

  test("it returns the formatted date when dateOnly is true", () => {
    currentFormat = { datePattern: "yyyy-MM-dd" }
    const result = useTimestamp(ts, { dateOnly: true })
    expect(result).toBe("2026-03-07")
  })

  test("it returns the formatted time when timeOnly is true", () => {
    currentFormat = { timePattern: "HH:mm:ss" }
    const result = useTimestamp(ts, { timeOnly: true })
    expect(result).toBe("09:08:07")
  })

  const dateCases = [
    { pattern: "yyyy-MM-dd", expected: "2026-03-07" },
    { pattern: "MM/dd/yyyy", expected: "03/07/2026" },
    { pattern: "dd/MM/yyyy", expected: "07/03/2026" },
    { pattern: "MMM d, yyyy", expected: "Mar 7, 2026" },
    { pattern: "d MMM yyyy", expected: "7 Mar 2026" },
  ]

  test.each(dateCases)("date pattern $pattern => $expected", ({
    pattern,
    expected,
  }) => {
    currentFormat = { datePattern: pattern }
    const result = useTimestamp(ts, { dateOnly: true })
    expect(result).toBe(expected)
  })

  // Parameterized time pattern tests (C# patterns)
  const timeCases = [
    { pattern: "h:mm tt", expected: "9:08 AM" },
    { pattern: "HH:mm", expected: "09:08" },
    { pattern: "h:mm:ss tt", expected: "9:08:07 AM" },
    { pattern: "HH:mm:ss", expected: "09:08:07" },
  ]

  test.each(timeCases)("time pattern $pattern => $expected", ({
    pattern,
    expected,
  }) => {
    currentFormat = { timePattern: pattern }
    const result = useTimestamp(ts, { timeOnly: true })
    expect(result).toBe(expected)
  })
})
