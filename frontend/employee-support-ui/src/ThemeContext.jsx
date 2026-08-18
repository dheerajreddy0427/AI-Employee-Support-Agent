import { createContext, useEffect, useMemo, useState } from "react";

export const ThemeContext = createContext({
  darkMode: false,
  toggleTheme: () => {},
});

export function ThemeProvider({ children }) {
  const [darkMode, setDarkMode] = useState(() => {
    try {
      return (localStorage.getItem("theme") || "light") === "dark";
    } catch {
      return false;
    }
  });

  useEffect(() => {
    const theme = darkMode ? "dark" : "light";
    document.documentElement.dataset.theme = theme;
    try { localStorage.setItem("theme", theme); } catch {}
  }, [darkMode]);

  const value = useMemo(
    () => ({ darkMode, toggleTheme: () => setDarkMode((d) => !d) }),
    [darkMode]
  );

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
}
