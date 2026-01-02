import { useLocation } from 'preact-iso';
import logo from '../assets/logo.svg';

export function Header() {
	const { url } = useLocation();

	return (
		<header>
			<nav class="p-4 flex flex-row justify-between">
				<a href="/" aria-label={'Home'}>
					<svg class="w-32" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 244 79">
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
					<a href="/" aria-label={'New'} class="btn btn-primary">
						<svg class="size-8" xmlns="http://www.w3.org/2000/svg" viewBox="0 -960 960 960">
							<path d="M440-440H200v-80h240v-240h80v240h240v80H520v240h-80v-240Z" />
						</svg>
					</a>
					<div class="avatar">
						<div class="size-12 rounded">
							<img src="https://img.daisyui.com/images/profile/demo/batperson@192.webp" />
						</div>
					</div>
				</div>
			</nav>
		</header>
	);
}
