<template>
  <div>
    <h3>{{ex.exceptionType}}: {{ex.message}}</h3>
    <h3>Stack Trace:</h3>
      <pre>
      {{ex.stackTrace}}
      </pre>
  </div>
</template>
<script>
export default {
  props: {
    ex: {
      type: Object
    }
  }
};
</script>
<style>
pre {
  white-space: pre-wrap;
  word-wrap: break-word;
}
</style>
