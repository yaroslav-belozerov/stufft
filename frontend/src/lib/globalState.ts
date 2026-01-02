import { signal, effect } from "@preact/signals";

export const token = signal(localStorage.getItem("token") || "");
export const editMode = signal(false);

effect(() => {
  if (token.value) {
    localStorage.setItem("token", token.value);
  } else {
    localStorage.removeItem("token");
  }
});

export const flipEditMode = () => (editMode.value = !editMode.value);
export const login = (newToken: string) => (token.value = newToken);
export const logout = () => {
  token.value = "";
  editMode.value = false;
};
