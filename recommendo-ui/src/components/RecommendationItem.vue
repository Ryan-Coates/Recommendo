<template>
<b-card no-body class="mb-1">
      <b-card-header header-tag="header" class="p-1" role="tab">
        <b-button block v-b-toggle.accordion-1 variant="info">{{recommendation.name}}</b-button>
      </b-card-header>
      <b-collapse id="accordion-1" visible accordion="my-accordion" role="tabpanel">
        <b-card v-bind:title=recommendation.name :sub-title=recommendation.recommender>
        <b-card-body>
          <b-card-text>{{recommendation.description}}</b-card-text>
        </b-card-body>
        </b-card>
      </b-collapse>
    </b-card>
</template>
<script>
export default {
  name: 'RecommendationItem',
  props: [
    'recommendation'
  ]
}
</script>
<style>
li{
    display: block;
    margin: 0 10px;
}
.btn-info{
    color: #fff;
    background-color: #263003;
    border-color: #77901C;
}
.btn-info:hover
{
    color: #fff;
    background-color: #263003;
    border-color: #E1F49D;
}
</style>
