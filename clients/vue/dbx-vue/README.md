# dbx-vue

Vue component package and demonstration application.  

## Overview
A Vue 3 composable/component package for client access to the Dbx Data Storage Extension API. Includes `Dbx Explorer` sample application.

## Quick Start
```sh
npm install @tudormobile/dbx-vue
```
### Import the composable
```html
<script setup>
import { useDbx } from '@tudormobile/dbx-vue';
const { listItems } = useDbx();
// ... use it as normal
</script>
```

### Use the component
```html
<script setup>
import { DbxManager } from '@tudormobile/dbx-vue';
</script>

<template>
  <DbxManager />
</template>
````

## Project Setup

```sh
npm install
```

### Compile and Hot-Reload for Development

```sh
npm run dev
```

### Type-Check, Compile and Minify for Production

```sh
npm run build
```
