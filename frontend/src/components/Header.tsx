import { useLocation } from "preact-iso";
import { useLocalStorage } from "../lib/useLocalStorage";
import { useState } from "preact/hooks";
import { GlobalState } from "./GlobalState";
import { editMode, flipEditMode, token } from "../lib/globalState";

export function Header() {
  return (
    <header>
      <nav class="p-4 flex flex-row justify-between">
        <a href="/" aria-label={"Home"}>
          <svg
            class="w-32"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 244 79"
          >
            <path
              fill="currentColor"
              d="M68 22h14v13H68v44H55V35H22V22h33V0h13v22Zm176 57h-46V66h33V35h-33V22h33V0h13v79ZM171 0v13h-25v9h25v13h-25v44h-13V0h38Z"
            />
            <path
              fill="currentColor"
              d="M225 13h-35v31h35v13h-35v22h-37V66h24v-9h-24V44h24V0h48v13ZM104 57H89v9h23V0h13v79H76V44h15V13H76V0h28v57ZM45 13H13v31h34v35H0V66h34v-9H0V0h45v13Z"
            />
          </svg>
        </a>
        <div class="flex flex-row items-center gap-4">
          {token.value != "" && (
            <button
              onClick={() => {
                flipEditMode();
              }}
              aria-label={"New"}
              class="btn btn-primary"
            >
              {editMode.value ? (
                <svg
                  class="size-8"
                  xmlns="http://www.w3.org/2000/svg"
                  viewBox="0 -960 960 960"
                >
                  <path d="M382-240 154-468l57-57 171 171 367-367 57 57-424 424Z" />
                </svg>
              ) : (
                <svg
                  class="size-8"
                  xmlns="http://www.w3.org/2000/svg"
                  viewBox="0 -960 960 960"
                >
                  <path d="M200-200h57l391-391-57-57-391 391v57Zm-80 80v-170l528-527q12-11 26.5-17t30.5-6q16 0 31 6t26 18l55 56q12 11 17.5 26t5.5 30q0 16-5.5 30.5T817-647L290-120H120Zm640-584-56-56 56 56Zm-141 85-28-29 57 57-29-28Z" />
                </svg>
              )}
            </button>
          )}
          <a href="/account" class="avatar">
            <div class="size-12 rounded">
              <img src="https://img.daisyui.com/images/profile/demo/batperson@192.webp" />
            </div>
          </a>
        </div>
      </nav>
    </header>
  );
}
