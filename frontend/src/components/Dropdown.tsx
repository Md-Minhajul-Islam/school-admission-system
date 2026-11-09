import {
  useEffect,
  useRef,
  useState,
  type KeyboardEvent as ReactKeyboardEvent,
} from "react";

export interface DropdownOption {
  value: string;
  label: string;
  disabled?: boolean;
}

interface DropdownProps {
  id: string;
  labelId: string;
  value: string;
  placeholder: string;
  options: DropdownOption[];
  onChange: (value: string) => void;
  disabled?: boolean;
}

type OpenPosition = "selected" | "first" | "last";

export function Dropdown({
  id,
  labelId,
  value,
  placeholder,
  options,
  onChange,
  disabled = false,
}: DropdownProps) {
  const [open, setOpen] = useState(false);
  const [activeIndex, setActiveIndex] = useState(-1);
  const containerRef = useRef<HTMLDivElement>(null);
  const triggerRef = useRef<HTMLButtonElement>(null);

  const selectedIndex = options.findIndex((option) => option.value === value);
  const selectedOption =
    selectedIndex >= 0 && !options[selectedIndex].disabled
      ? options[selectedIndex]
      : undefined;

  function firstEnabledIndex() {
    return options.findIndex((option) => !option.disabled);
  }

  function lastEnabledIndex() {
    for (let index = options.length - 1; index >= 0; index -= 1) {
      if (!options[index].disabled) {
        return index;
      }
    }

    return -1;
  }

  function openList(position: OpenPosition = "selected") {
    if (disabled) {
      return;
    }

    setOpen(true);

    if (position === "first") {
      setActiveIndex(firstEnabledIndex());
      return;
    }

    if (position === "last") {
      setActiveIndex(lastEnabledIndex());
      return;
    }

    setActiveIndex(
      selectedOption ? selectedIndex : firstEnabledIndex(),
    );
  }

  function closeList() {
    setOpen(false);
  }

  function moveActive(direction: 1 | -1) {
    if (options.length === 0) {
      return;
    }

    let index = activeIndex;
    for (let step = 0; step < options.length; step += 1) {
      index = (index + direction + options.length) % options.length;
      if (!options[index].disabled) {
        setActiveIndex(index);
        return;
      }
    }
  }

  function selectOption(option: DropdownOption | undefined) {
    if (!option || option.disabled) {
      return;
    }

    onChange(option.value);
    setOpen(false);
    triggerRef.current?.focus();
  }

  function handleKeyDown(event: ReactKeyboardEvent<HTMLButtonElement>) {
    switch (event.key) {
      case "ArrowDown":
        event.preventDefault();
        if (open) {
          moveActive(1);
        } else {
          openList("first");
        }
        break;
      case "ArrowUp":
        event.preventDefault();
        if (open) {
          moveActive(-1);
        } else {
          openList("last");
        }
        break;
      case "Home":
        if (!open) {
          return;
        }
        event.preventDefault();
        setActiveIndex(firstEnabledIndex());
        break;
      case "End":
        if (!open) {
          return;
        }
        event.preventDefault();
        setActiveIndex(lastEnabledIndex());
        break;
      case "Enter":
      case " ":
        event.preventDefault();
        if (open) {
          selectOption(options[activeIndex]);
        } else {
          openList();
        }
        break;
      case "Escape":
        if (!open) {
          return;
        }
        event.preventDefault();
        closeList();
        break;
      case "Tab":
        closeList();
        break;
      default:
        break;
    }
  }

  useEffect(() => {
    if (!open) {
      return;
    }

    function handlePointerDown(event: PointerEvent) {
      if (!containerRef.current?.contains(event.target as Node)) {
        setOpen(false);
      }
    }

    document.addEventListener("pointerdown", handlePointerDown);
    return () => document.removeEventListener("pointerdown", handlePointerDown);
  }, [open]);

  useEffect(() => {
    if (!open || activeIndex < 0) {
      return;
    }

    document
      .getElementById(`${id}-option-${activeIndex}`)
      ?.scrollIntoView({ block: "nearest" });
  }, [activeIndex, id, open]);

  useEffect(() => {
    if (disabled) {
      setOpen(false);
    }
  }, [disabled]);

  return (
    <div ref={containerRef} className="relative">
      <button
        ref={triggerRef}
        id={id}
        type="button"
        role="combobox"
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-controls={`${id}-listbox`}
        aria-activedescendant={
          open && activeIndex >= 0 ? `${id}-option-${activeIndex}` : undefined
        }
        disabled={disabled}
        onClick={() => {
          if (open) {
            closeList();
          } else {
            openList();
          }
        }}
        onKeyDown={handleKeyDown}
        className={`flex w-full items-center justify-between gap-3 rounded-xl border px-3.5 py-2.5 text-left text-sm shadow-sm outline-none transition ${
          disabled
            ? "cursor-not-allowed border-slate-300 bg-slate-100 text-slate-500"
            : "border-slate-300 bg-white text-slate-900 hover:border-emerald-300 focus:border-emerald-400 focus:ring-4 focus:ring-emerald-100"
        }`}
      >
        <span
          className={`min-w-0 truncate ${selectedOption ? "" : "text-slate-500"}`}
        >
          {selectedOption?.label ?? placeholder}
        </span>
        <svg
          aria-hidden="true"
          viewBox="0 0 20 20"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.8"
          className={`h-4 w-4 shrink-0 text-slate-400 transition-transform ${
            open ? "rotate-180" : ""
          }`}
        >
          <path d="M6 8l4 4 4-4" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      </button>

      {open ? (
        <ul
          id={`${id}-listbox`}
          role="listbox"
          aria-labelledby={labelId}
          className="absolute left-0 right-0 z-40 mt-2 max-h-64 overflow-y-auto overscroll-contain rounded-2xl border border-emerald-200 bg-white p-1.5 shadow-2xl shadow-emerald-950/10 ring-1 ring-emerald-100/70"
        >
          {options.length === 0 ? (
            <li className="px-3 py-2 text-sm text-slate-500">
              No options available
            </li>
          ) : (
            options.map((option, index) => {
              const selected = option.value === value;
              const active = index === activeIndex;

              return (
                <li
                  key={option.value}
                  id={`${id}-option-${index}`}
                  role="option"
                  aria-selected={selected}
                  aria-disabled={option.disabled || undefined}
                  onMouseDown={(event) => event.preventDefault()}
                  onMouseEnter={() => {
                    if (!option.disabled) {
                      setActiveIndex(index);
                    }
                  }}
                  onClick={() => selectOption(option)}
                  className={`flex items-center justify-between gap-2 rounded-lg px-3 py-2 text-sm transition-colors ${
                    option.disabled
                      ? "cursor-not-allowed text-slate-400"
                      : selected
                        ? "cursor-pointer bg-emerald-50 font-semibold text-emerald-800"
                        : active
                          ? "cursor-pointer bg-emerald-50/70 text-slate-900"
                          : "cursor-pointer text-slate-700"
                  }`}
                >
                  <span className="min-w-0 break-words">{option.label}</span>
                  {selected ? (
                    <svg
                      aria-hidden="true"
                      viewBox="0 0 20 20"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                      className="h-4 w-4 shrink-0 text-emerald-600"
                    >
                      <path
                        d="M5 10.5l3.5 3.5L15 6.5"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />
                    </svg>
                  ) : null}
                </li>
              );
            })
          )}
        </ul>
      ) : null}
    </div>
  );
}
