<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useDbx } from '../composables/useDbx';

const { getStatus, listItems } = useDbx("http://127.0.0.1:5284/api/v1");

const id = ref('test');
const items = ref<string[]>([]);
const status = ref<any>(null);

const fetchStatus = async () => {
    const res = await getStatus();
    if (res.success) status.value = res.data;
};

const refreshList = async () => {
    if (!id.value) return;
    const res = await listItems(id.value);
    if (res.success) {
        items.value = res.data || [];
    } else {
        items.value = [];
    }
};

onMounted(() => {
    fetchStatus();
    refreshList();
});

watch(id, () => {
    refreshList();
});

</script>

<template>
    <div>
        <div>
            <button @click="refreshList" class="hover:rotate-180 transition-transform duration-500">
                Refresh View
            </button>
        </div>
        <div v-for="item_id in items" :key="item_id">
            <div>{{ item_id }}</div>
        </div>      
    </div>

</template>

<style scoped></style>