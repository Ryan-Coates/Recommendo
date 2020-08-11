
<template>
    <div id="example">
        <h1>{{ type }}</h1>

        <div v-if="isLoading" class="article-preview">Loading articles...</div>
        <div v-else>
          <div v-if="articles.length === 0" class="article-preview">
            No articles are here... yet.
          </div>
          <RwvArticlePreview
            v-for="(article, index) in articles"
            :article="article"
            :key="article.title + index"
          />
          <VPagination :pages="pages" :currentPage.sync="currentPage" />
        </div>
        <p>{{ info }}</p>
    </div>
</template>
<script lang="ts">
import axios from 'axios'
import { Component, Prop, Vue } from 'vue-property-decorator'

export default {
  data () {
    return {
      info: ''
    }
  },
  created () {
    axios.get('https://api.coindesk.com/v1/bpi/currentprice.json')
      .then(response => { this.info = response.data })
  },
  props: {
    type: String
  }
}
</script>
