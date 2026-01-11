import { useLocation } from "preact-iso";
import { useEffect, useState } from "preact/hooks";
import { useLocalStorage } from "../../lib/useLocalStorage";
import { GlobalState } from "../../components/GlobalState";
import { editMode, token } from "../../lib/globalState";
import { ENDPOINT } from "../../lib/const";
import { setDefaultCACertificates } from "tls";
import { title } from "process";
import { effect } from "@preact/signals";

export function Home() {
  let [cards, setCards] = useState<Card[] | null>(null);
  let [tags, setTags] = useState<Set<string>>(new Set());
  let [currentTags, setCurrentTags] = useState<Set<string>>(new Set());

  const updateCards = async (resp: Response) => {
    if (resp.ok) {
      let body: Card[] = await resp.json();
      setCards(body);
      setTags(new Set(body.map((c) => c.tags.map((it) => it.content)).flat()));
    }
  };

  const clickTag = (it: string) => {
    if (currentTags.has(it)) {
      setCurrentTags(new Set([...currentTags].filter((t) => t != it)));
    } else {
      setCurrentTags(new Set([...currentTags, it]));
    }
  };

  const [draft, setDraft] = useState({ title: "", textContent: "" });
  const addCard = () => {
    setDraft({ title: "", textContent: "" });
    fetch(`${ENDPOINT}/cards`, {
      headers: [
        ["Content-Type", "application/json"],
        ["Authorization", `Bearer ${token.value}`],
      ],
      method: "POST",
      body: JSON.stringify(draft),
    }).then(updateCards);
  };

  const [edited, setEdited] = useState<Set<Number>>(new Set());

  const updateCard = (id: Number, newCard: Card) => {
    console.log("im updating lol");
    setEdited(edited.union(new Set([id])));
    console.log(edited);
    setCards(cards.map((it) => (it.id == id ? newCard : it)));
  };

  const updateAllCards = () => {
    console.log("UPDATING ALL???: ", edited);
    if (edited.size == 0) return;
    console.log("UPDATING ALL FR???: ", edited);
    fetch(`${ENDPOINT}/update_all`, {
      headers: [
        ["Content-Type", "application/json"],
        ["Authorization", `Bearer ${token.value}`],
      ],
      method: "POST",
      body: JSON.stringify(
        cards
          .filter((it) => edited.includes(it.id))
          .map((it) => {
            return {
              id: it.id,
              title: it.title,
              textContent: it.textContent,
            };
          }),
      ),
    }).then(updateCards);
    setEdited(new Set());
  };

  const mapCards = (cs: Card[]) => {
    return cs
      .filter(
        (it) =>
          currentTags.size == 0 ||
          new Set(it.tags).intersection(currentTags).size != 0,
      )
      .map((it) =>
        card(it, editMode.value, (newc) => {
          updateCard(it.id, newc);
        }),
      );
  };

  useEffect(() => {
    fetchCards().then(updateCards);
    effect(() => {
      if (editMode.value == false) {
        updateAllCards();
      }
    });
  }, []);

  if (cards) {
    return (
      <div class="px-4 flex flex-col gap-4">
        <div class="flex flex-row gap-2 flex-wrap">
          {[...tags].map((it) => (
            <button
              class={`cursor-pointer badge ${currentTags.has(it) ? "" : "badge-soft"} badge-primary`}
              onClick={() => clickTag(it)}
            >
              {it}
            </button>
          ))}
        </div>
        <div class="flex flex-row flex-wrap gap-4">
          <div class="card group bg-base-200 min-w-72 max-w-96 h-fit shadow-sm cursor-pointer hover:scale-105 hover:-rotate-2 transition-all">
            <div class="card-body">
              <div class="flex flex-col gap-2">
                <input
                  type="text"
                  defaultValue={draft.title}
                  placeholder="Card title here"
                  class="input input-lg"
                  onInput={(it) => {
                    setDraft({ ...draft, title: it.target.value });
                  }}
                />
                <input
                  type="text"
                  defaultValue={draft.textContent}
                  placeholder="Card content here"
                  class="input"
                  onInput={(it) => {
                    setDraft({ ...draft, textContent: it.target.value });
                  }}
                />
                <button class="btn btn-primary" onClick={addCard}>
                  Add
                </button>
              </div>
            </div>
          </div>
          {mapCards(cards)}
        </div>
      </div>
    );
  } else {
    return <div class="loading loading-spinner ms-4"></div>;
  }
}

type Card = {
  id: Number;
  title: string;
  textContent: string;
  links: { id: Number; type: string; content: string }[];
  tags: { content: string }[];
};

async function fetchCards(): Promise<Response> {
  return await fetch(`${ENDPOINT}/cards`, {
    headers: [
      ["Content-Type", "application/json"],
      ["Authorization", `Bearer ${token.value}`],
    ],
  });
}

function getIcon(url: string) {
  const githubRegex = RegExp(".*github.com.*");
  if (githubRegex.test(url)) {
    return (
      <svg
        class="size-8"
        viewBox="0 0 100 100"
        xmlns="http://www.w3.org/2000/svg"
      >
        <path
          fill-rule="evenodd"
          clip-rule="evenodd"
          d="M48.854 0C21.839 0 0 22 0 49.217c0 21.756 13.993 40.172 33.405 46.69 2.427.49 3.316-1.059 3.316-2.362 0-1.141-.08-5.052-.08-9.127-13.59 2.934-16.42-5.867-16.42-5.867-2.184-5.704-5.42-7.17-5.42-7.17-4.448-3.015.324-3.015.324-3.015 4.934.326 7.523 5.052 7.523 5.052 4.367 7.496 11.404 5.378 14.235 4.074.404-3.178 1.699-5.378 3.074-6.6-10.839-1.141-22.243-5.378-22.243-24.283 0-5.378 1.94-9.778 5.014-13.2-.485-1.222-2.184-6.275.486-13.038 0 0 4.125-1.304 13.426 5.052a46.97 46.97 0 0 1 12.214-1.63c4.125 0 8.33.571 12.213 1.63 9.302-6.356 13.427-5.052 13.427-5.052 2.67 6.763.97 11.816.485 13.038 3.155 3.422 5.015 7.822 5.015 13.2 0 18.905-11.404 23.06-22.324 24.283 1.78 1.548 3.316 4.481 3.316 9.126 0 6.6-.08 11.897-.08 13.526 0 1.304.89 2.853 3.316 2.364 19.412-6.52 33.405-24.935 33.405-46.691C97.707 22 75.788 0 48.854 0z"
          fill="currentColor"
        />
      </svg>
    );
  }

  const wikipediaRegex = RegExp(".*wiki[media|pedia].*");
  if (wikipediaRegex.test(url)) {
    return (
      <svg
        class="size-8"
        fill="currentColor"
        xmlns="http://www.w3.org/2000/svg"
        viewBox="0 30 130 72"
      >
        <path d="M 120.85,29.21 C 120.85,29.62 120.72,29.99 120.47,30.33 C 120.21,30.66 119.94,30.83 119.63,30.83 C 117.14,31.07 115.09,31.87 113.51,33.24 C 111.92,34.6 110.29,37.21 108.6,41.05 L 82.8,99.19 C 82.63,99.73 82.16,100 81.38,100 C 80.77,100 80.3,99.73 79.96,99.19 L 65.49,68.93 L 48.85,99.19 C 48.51,99.73 48.04,100 47.43,100 C 46.69,100 46.2,99.73 45.96,99.19 L 20.61,41.05 C 19.03,37.44 17.36,34.92 15.6,33.49 C 13.85,32.06 11.4,31.17 8.27,30.83 C 8,30.83 7.74,30.69 7.51,30.4 C 7.27,30.12 7.15,29.79 7.15,29.42 C 7.15,28.47 7.42,28 7.96,28 C 10.22,28 12.58,28.1 15.05,28.3 C 17.34,28.51 19.5,28.61 21.52,28.61 C 23.58,28.61 26.01,28.51 28.81,28.3 C 31.74,28.1 34.34,28 36.6,28 C 37.14,28 37.41,28.47 37.41,29.42 C 37.41,30.36 37.24,30.83 36.91,30.83 C 34.65,31 32.87,31.58 31.57,32.55 C 30.27,33.53 29.62,34.81 29.62,36.4 C 29.62,37.21 29.89,38.22 30.43,39.43 L 51.38,86.74 L 63.27,64.28 L 52.19,41.05 C 50.2,36.91 48.56,34.23 47.28,33.03 C 46,31.84 44.06,31.1 41.46,30.83 C 41.22,30.83 41,30.69 40.78,30.4 C 40.56,30.12 40.45,29.79 40.45,29.42 C 40.45,28.47 40.68,28 41.16,28 C 43.42,28 45.49,28.1 47.38,28.3 C 49.2,28.51 51.14,28.61 53.2,28.61 C 55.22,28.61 57.36,28.51 59.62,28.3 C 61.95,28.1 64.24,28 66.5,28 C 67.04,28 67.31,28.47 67.31,29.42 C 67.31,30.36 67.15,30.83 66.81,30.83 C 62.29,31.14 60.03,32.42 60.03,34.68 C 60.03,35.69 60.55,37.26 61.6,39.38 L 68.93,54.26 L 76.22,40.65 C 77.23,38.73 77.74,37.11 77.74,35.79 C 77.74,32.69 75.48,31.04 70.96,30.83 C 70.55,30.83 70.35,30.36 70.35,29.42 C 70.35,29.08 70.45,28.76 70.65,28.46 C 70.86,28.15 71.06,28 71.26,28 C 72.88,28 74.87,28.1 77.23,28.3 C 79.49,28.51 81.35,28.61 82.8,28.61 C 83.84,28.61 85.38,28.52 87.4,28.35 C 89.96,28.12 92.11,28 93.83,28 C 94.23,28 94.43,28.4 94.43,29.21 C 94.43,30.29 94.06,30.83 93.32,30.83 C 90.69,31.1 88.57,31.83 86.97,33.01 C 85.37,34.19 83.37,36.87 80.98,41.05 L 71.26,59.02 L 84.42,85.83 L 103.85,40.65 C 104.52,39 104.86,37.48 104.86,36.1 C 104.86,32.79 102.6,31.04 98.08,30.83 C 97.67,30.83 97.47,30.36 97.47,29.42 C 97.47,28.47 97.77,28 98.38,28 C 100.03,28 101.99,28.1 104.25,28.3 C 106.34,28.51 108.1,28.61 109.51,28.61 C 111,28.61 112.72,28.51 114.67,28.3 C 116.7,28.1 118.52,28 120.14,28 C 120.61,28 120.85,28.4 120.85,29.21 z" />
      </svg>
    );
  }

  return (
    <svg
      class="size-7"
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 -960 960 960"
      fill="currentColor"
    >
      <path d="M200-120q-33 0-56.5-23.5T120-200v-560q0-33 23.5-56.5T200-840h280v80H200v560h560v-280h80v280q0 33-23.5 56.5T760-120H200Zm188-212-56-56 372-372H560v-80h280v280h-80v-144L388-332Z" />
    </svg>
  );
}

function card(data: Card, editMode: boolean, updateCard: (Card) => void) {
  const [flipped, setFlipped] = useState(false);

  return (
    <button
      onClick={() => {
        if (!editMode) {
          setFlipped(!flipped);
        }
      }}
      class="card group bg-base-200 min-w-72 max-w-96 h-fit shadow-sm cursor-pointer hover:scale-105 hover:-rotate-2 transition-all"
    >
      <div class="card-body">
        {/*<img class="max-w-32 card" src={data.img}></img>*/}
        {editMode ? (
          <div class="flex flex-col gap-2">
            <input
              type="text"
              defaultValue={data.title}
              placeholder="Card title here"
              class="input input-lg"
              onInput={(it) => {
                updateCard({ ...data, title: it.target.value });
              }}
            />
            <input
              type="text"
              defaultValue={data.textContent}
              placeholder="Card content here"
              class="input"
              onInput={(it) => {
                updateCard({ ...data, content: it.target.value });
              }}
            />
          </div>
        ) : (
          <h2 class="font-bold text-start text-3xl text-ellipsis overflow-hidden">
            {data.title}
          </h2>
        )}
      </div>
      {!editMode && (
        <div
          class={`card leading-6 text-lg p-4 absolute bg-base-200 min-h-full transition-all top-0 left-0 right-0 opacity-0 ${flipped ? "opacity-100" : "pointer-events-none"}`}
        >
          {data.textContent}
          <div class="flex flex-row gap-1 justify-center">
            {data.links.map((it) => (
              <a
                target="_blank"
                href={it.content}
                class={`p-2 flex flex-col items-center justify-center hover:bg-base-300 rounded-full transition-all`}
              >
                {getIcon(it.content)}
              </a>
            ))}
          </div>
        </div>
      )}
    </button>
  );
}
