<template>
  <div class="search-input"></div>
</template>
<script>
export default {
  name: "SearchInput",
  data: () => ({
    loaded: false,
    loading: false,
  }),
  methods: {
    onClick() {
      this.loading = true;
      setTimeout(() => {
        this.loading = false;
        this.loaded = true;
      }, 2000);
    },
  },
};
</script>
<style scoped>
.search-input {
  width: 400px;
}
@media screen and (max-width: 1023px) and (min-width: 740px) {
  .search-input {
    display: none;
  }
}
@media screen and (max-width: 739px) {
  .search-input {
    display: none;
  }
}
</style>
