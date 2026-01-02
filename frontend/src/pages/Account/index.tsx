import { useEffect, useState } from "preact/hooks";
import { useLocalStorage } from "../../lib/useLocalStorage";
import { ENDPOINT } from "../../lib/const";
import { getCookieValue } from "../../lib/getCookieValue";
import { GlobalState } from "../../components/GlobalState";
import { login, logout, token } from "../../lib/globalState";

export default function Account() {
  let [refresh, setRefresh] = useState<boolean>(false);
  let [userData, setUserData] = useState<{
    displayName: string;
    username: string;
    accountCreated: Date;
  } | null>(null);

  const setTokens = async (response: Response) => {
    setRefresh(response.ok);
    if (response.ok) {
      const body = await response.json();
      login(body.accessToken);
    }
  };

  const setUser = async (response: Response) => {
    if (!response.ok) {
      logout();
      setRefresh(false);
    } else {
      const body = await response.json();
      setUserData(body);
    }
  };

  const doRefresh = () => {
    fetch(`${ENDPOINT}/auth/refresh`, {
      headers: [["Content-Type", "application/json"]],
      method: "POST",
      credentials: "include",
    }).then(setTokens);
  };

  const doLogout = () => {
    fetch(`${ENDPOINT}/auth/logout`, {
      headers: [
        ["Content-Type", "application/json"],
        ["Authorization", `Bearer ${token.value}`],
      ],
      method: "POST",
      credentials: "include",
    }).then((it) => {
      if (it.ok) {
        logout();
        setUserData(null);
        setRefresh(false);
      }
    });
  };

  useEffect(() => {
    fetch(`${ENDPOINT}/user`, {
      headers: [
        ["Content-Type", "application/json"],
        ["Authorization", `Bearer ${token.value}`],
      ],
    }).then(setUser);
  }, [token.value]);

  if (token.value == "") {
    if (refresh == false) {
      return loginForm((token) => {
        setRefresh(true);
        login(token);
      });
    } else {
      return <div class="loading loading-spinner"></div>;
    }
  } else {
    return (
      <div class="flex flex-col gap-2 px-4">
        {userData && (
          <div>
            {userData.displayName} - {userData.username} -{" "}
            {userData.accountCreated}
          </div>
        )}
        <button class="btn btn-error" onClick={doLogout}>
          Logout
        </button>
      </div>
    );
  }
}

function loginForm(onSuccessToken: (token: string) => void) {
  const [username, setUsername] = useState("");
  const [displayName, setDisplay] = useState("");
  const [password, setPassword] = useState("");
  const [isReg, setIsReg] = useState(false);
  const [err, setErr] = useState(-1);
  const [registeredUser, setRegisteredUser] = useState("");

  const setRegistered = async (response: Response) => {
    if (response.ok) {
      const body = await response.json();
      setRegisteredUser(body.username);
      setIsReg(false);
      setErr(-1);
    } else {
      setErr(response.status);
    }
  };

  const setLoggedIn = async (response: Response) => {
    if (response.ok) {
      const body = await response.json();
      onSuccessToken(body.accessToken);
      setRegisteredUser("");
    } else {
      setErr(response.status);
    }
  };

  const doLogin = () => {
    if (isReg) {
      fetch(`${ENDPOINT}/auth/reg`, {
        method: "POST",
        headers: [["Content-Type", "application/json"]],
        body: JSON.stringify({
          username,
          displayName,
          password,
        }),
      }).then(setRegistered);
    } else {
      fetch(`${ENDPOINT}/auth/login`, {
        method: "POST",
        headers: [["Content-Type", "application/json"]],
        body: JSON.stringify({
          username,
          password,
        }),
        credentials: "include",
      }).then(setLoggedIn);
    }
  };

  const getErrMessage = (code: number) => {
    if (code >= 500) {
      return "Something went wrong";
    }

    switch (code) {
      case 404:
        return "User not found";
      case 401:
        return "Wrong credentials";
      case 409:
        return "User already exists";
    }

    return "";
  };

  return (
    <form
      class="flex flex-col gap-3 w-full items-center"
      onSubmit={(e) => {
        e.preventDefault();
        doLogin();
      }}
    >
      <h2 class="text-3xl">{isReg ? "Register" : "Sign back in"}</h2>
      {getErrMessage(err) && (
        <h3 class="text-error text-xl">{getErrMessage(err)}</h3>
      )}
      <h3 class="text-accent text-xl">
        {registeredUser != "" &&
          `Welcome, ${registeredUser}! Now please sign in.`}
      </h3>
      {isReg && (
        <input
          type="text"
          placeholder="Display Name"
          class="input input-lg"
          onInput={(it) => {
            setDisplay(it.target.value);
          }}
        />
      )}
      <input
        type="text"
        placeholder="Login"
        class="input input-lg"
        onInput={(it) => {
          setUsername(it.target.value);
        }}
      />
      <input
        type="text"
        placeholder="Password"
        class="input input-lg"
        onInput={(it) => {
          setPassword(it.target.value);
        }}
      />
      <div class="flex flex-row gap-2">
        <button
          onClick={() => setIsReg(!isReg)}
          type="button"
          class="btn btn-secondary"
        >
          {isReg ? "Have an account?" : "New user?"}
        </button>
        <input
          disabled={
            !(
              username != "" &&
              password != "" &&
              (isReg ? displayName != "" : true)
            )
          }
          type="submit"
          class="btn btn-primary"
          value={isReg ? "Sign up" : "Sign in"}
        />
      </div>
    </form>
  );
}
